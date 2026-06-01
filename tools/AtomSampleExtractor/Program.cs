using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

// =============================================================================
// AtomEventStore stratified sample extractor
//
// Lists Atom-feed blobs under a known set of sub-path prefixes, then downloads a
// bounded, representative subset stratified by:
//   * content type     - the qualified name of the <content> child element
//   * structural shape  - the set of Atom link rels + entry count
//   * temporal extremes - earliest + latest blobs (to catch schema drift)
// and writes the kept samples (raw bytes) plus a manifest.csv to an output dir.
//
// The SAS URL is read from a gitignored file (--sas-file) or the
// ATOM_SAMPLER_SAS_URL env var. It is never written to stdout or the manifest.
// =============================================================================

var opts = Options.Parse(args);
if (opts is null)
{
    Options.PrintUsage();
    return 1;
}

string? sasUrl = ResolveSasUrl(opts);
if (string.IsNullOrWhiteSpace(sasUrl))
{
    Console.Error.WriteLine(
        "ERROR: no SAS URL. Provide --sas-file <path> or set ATOM_SAMPLER_SAS_URL. " +
        "The SAS must grant read + list (rl) on the container.");
    return 1;
}

var container = CreateContainerClient(sasUrl, opts.Container, out var clientError);
if (container is null)
{
    Console.Error.WriteLine($"ERROR: {clientError}");
    return 1;
}

// ---- Probe mode: list raw names only, no downloads (structure discovery) ----
if (opts.Probe)
{
    foreach (var prefix in opts.Prefixes)
    {
        Console.WriteLine($"-- Probing '{(prefix.Length == 0 ? "(root)" : prefix)}' --");
        int shown = 0, scanned = 0;
        var capped = false;
        var tally = new Dictionary<string, int>(StringComparer.Ordinal);
        try
        {
            await foreach (var b in container.GetBlobsAsync(prefix: prefix))
            {
                if (shown < opts.ProbeTake) { Console.WriteLine($"  {b.Name}"); shown++; }
                var key = string.Join("/", b.Name.Split('/').Take(3));
                tally[key] = tally.GetValueOrDefault(key) + 1;
                if (++scanned >= 5000) { capped = true; break; }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  ERROR listing: {ex.Message} (does the SAS include LIST / 'l'?)");
            continue;
        }
        Console.WriteLine($"  ... scanned {scanned}{(capped ? "+ (capped at 5000)" : "")} names.");
        Console.WriteLine("  Distinct first-3-segment prefixes (count):");
        foreach (var kv in tally.OrderByDescending(k => k.Value).Take(20))
            Console.WriteLine($"    {kv.Value,7}  {kv.Key}");
        Console.WriteLine();
    }
    Console.WriteLine("Probe complete (no blobs downloaded). Use the exact prefixes above for extraction.");
    return 0;
}

Console.WriteLine($"Container : {container.Uri.GetLeftPart(UriPartial.Path)}");
Console.WriteLine($"Prefixes  : {string.Join(", ", opts.Prefixes.Select(p => p.Length == 0 ? "(root)" : p))}");
Console.WriteLine($"Output    : {Path.GetFullPath(opts.OutDir)}");
Console.WriteLine($"Per bucket: {opts.PerBucket}   Extremes: {opts.Extremes}   " +
                  $"Max downloads: {opts.MaxDownloads}   List cap/prefix: {opts.ListCap}");
Console.WriteLine();

// ---- Phase 1: list (metadata only, cheap) -----------------------------------
var items = new List<BlobItem>();
var listingTruncated = false;
try
{
    foreach (var prefix in opts.Prefixes)
    {
        var countForPrefix = 0;
        await foreach (var b in container.GetBlobsAsync(prefix: prefix))
        {
            if (!b.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                continue;
            items.Add(b);
            if (++countForPrefix >= opts.ListCap)
            {
                listingTruncated = true;
                Console.Error.WriteLine(
                    $"WARN: listing capped at {opts.ListCap} for prefix " +
                    $"'{(prefix.Length == 0 ? "(root)" : prefix)}' - more blobs exist; " +
                    "coverage may be incomplete. Raise --list-cap to widen.");
                break;
            }
        }
        Console.WriteLine($"Listed {countForPrefix,8} .xml blobs under '{(prefix.Length == 0 ? "(root)" : prefix)}'.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        $"ERROR: listing failed ({ex.Message}). Does the SAS include LIST permission (rl)?");
    return 1;
}

if (items.Count == 0)
{
    Console.Error.WriteLine("No .xml blobs found under the given prefixes. Check the prefixes and SAS scope.");
    return 1;
}

// ---- Phase 2: choose download candidates (bounded) --------------------------
var candidates = new List<Candidate>();
var seen = new HashSet<string>(StringComparer.Ordinal);

void AddCandidate(BlobItem b, string reason)
{
    if (seen.Add(b.Name))
        candidates.Add(new Candidate(b.Name, b.Properties.LastModified, b.Properties.ContentLength, reason));
}

// Temporal extremes first (always downloaded + always kept).
var byTime = items
    .Where(i => i.Properties.LastModified.HasValue)
    .OrderBy(i => i.Properties.LastModified!.Value)
    .ToList();
foreach (var b in byTime.Take(opts.Extremes)) AddCandidate(b, "earliest");
foreach (var b in byTime.TakeLast(opts.Extremes)) AddCandidate(b, "latest");

// Spread sample across the name space to diversify content types / shapes,
// deterministic (stride) so the corpus is reproducible.
var byName = items.OrderBy(i => i.Name, StringComparer.Ordinal).ToList();
var spreadBudget = Math.Max(0, opts.MaxDownloads - candidates.Count);
if (spreadBudget > 0)
{
    var stride = Math.Max(1, byName.Count / spreadBudget);
    for (var i = 0; i < byName.Count && candidates.Count < opts.MaxDownloads; i += stride)
        AddCandidate(byName[i], "spread");
}

Console.WriteLine();
Console.WriteLine($"Selected {candidates.Count} download candidates " +
                  $"(from {items.Count} listed). Downloading...");
Console.WriteLine();

// ---- Phase 3: download, parse, bucket, keep ---------------------------------
Directory.CreateDirectory(opts.OutDir);

var manifest = new StringBuilder();
manifest.AppendLine("blobName,lastModified,length,contentType,linkRels,entryCount,bucket,reason,kept");

var bucketKept = new Dictionary<string, int>(StringComparer.Ordinal);
var bucketSeen = new Dictionary<string, int>(StringComparer.Ordinal);
int downloaded = 0, written = 0, parsedAtom = 0, skippedNonAtom = 0, skippedOversize = 0, failed = 0;
var budgetHit = false;

foreach (var c in candidates)
{
    if (downloaded >= opts.MaxDownloads)
    {
        budgetHit = true;
        break;
    }

    if (c.Length is > Options.MaxBlobBytes)
    {
        skippedOversize++;
        continue;
    }

    byte[] bytes;
    try
    {
        var resp = await BlobClientFor(container, sasUrl, c.Name).DownloadContentAsync();
        bytes = resp.Value.Content.ToArray();
        downloaded++;
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"WARN: download failed for '{c.Name}': {ex.Message.Split('\n')[0]}");
        continue;
    }

    var info = Feed.TryParse(bytes);
    if (info is null)
    {
        skippedNonAtom++;
        continue;
    }
    parsedAtom++;

    var bucket = $"{info.ContentType}|{info.LinkRels}|n={EntryBand(info.EntryCount)}";
    bucketSeen[bucket] = bucketSeen.GetValueOrDefault(bucket) + 1;

    var isExtreme = c.Reason is "earliest" or "latest";
    var keptSoFar = bucketKept.GetValueOrDefault(bucket);
    var keep = isExtreme || keptSoFar < opts.PerBucket;

    if (keep)
    {
        var fileName = SafeFileName(c.Name);
        await File.WriteAllBytesAsync(Path.Combine(opts.OutDir, fileName), bytes);
        bucketKept[bucket] = keptSoFar + 1;
        written++;
    }

    manifest.AppendLine(string.Join(",",
        Csv(c.Name),
        Csv(c.LastModified?.ToString("o") ?? ""),
        c.Length?.ToString() ?? "",
        Csv(info.ContentType),
        Csv(info.LinkRels),
        info.EntryCount.ToString(),
        Csv(bucket),
        c.Reason,
        keep ? "yes" : "no"));
}

await File.WriteAllTextAsync(Path.Combine(opts.OutDir, "manifest.csv"), manifest.ToString());

// ---- Summary (no silent caps) ----------------------------------------------
Console.WriteLine();
Console.WriteLine("==== Summary ====");
Console.WriteLine($"Listed blobs        : {items.Count}{(listingTruncated ? " (TRUNCATED - see warnings)" : "")}");
Console.WriteLine($"Downloaded          : {downloaded}");
Console.WriteLine($"  parsed as Atom    : {parsedAtom}");
Console.WriteLine($"  skipped non-Atom  : {skippedNonAtom}");
Console.WriteLine($"  skipped oversize  : {skippedOversize} (> {Options.MaxBlobBytes / (1024 * 1024)} MB)");
Console.WriteLine($"  failed            : {failed}");
Console.WriteLine($"Distinct buckets    : {bucketSeen.Count}");
Console.WriteLine($"Samples written     : {written} -> {Path.GetFullPath(opts.OutDir)}");
Console.WriteLine($"Manifest            : {Path.Combine(Path.GetFullPath(opts.OutDir), "manifest.csv")}");
if (budgetHit)
    Console.Error.WriteLine(
        $"WARN: download budget ({opts.MaxDownloads}) reached before all candidates were processed - " +
        "some buckets may be under-sampled. Raise --max-downloads to widen coverage.");
Console.WriteLine();
Console.WriteLine("Buckets discovered (content type | link rels | entry band -> seen/kept):");
foreach (var kv in bucketSeen.OrderByDescending(kv => kv.Value))
    Console.WriteLine($"  {kv.Value,5} / {bucketKept.GetValueOrDefault(kv.Key),-3}  {kv.Key}");

Console.WriteLine();
Console.WriteLine("REMINDER: extracted files are real production payloads. Review/scrub before");
Console.WriteLine("committing any of them as test fixtures. The output folder is gitignored.");
return 0;

// ---- local helpers ----------------------------------------------------------

static string? ResolveSasUrl(Options opts)
{
    if (opts.SasFile is { } file)
    {
        if (!File.Exists(file))
        {
            Console.Error.WriteLine($"ERROR: --sas-file not found: {file}");
            return null;
        }
        return File.ReadAllText(file).Trim();
    }
    return Environment.GetEnvironmentVariable("ATOM_SAMPLER_SAS_URL")?.Trim();
}

// Accepts any of: a container-scoped SAS URL (container in the path), a blob
// *service* SAS URL (account root, no container - needs --container), or a
// connection string (also needs --container).
static BlobContainerClient? CreateContainerClient(string sasOrConn, string? containerName, out string? error)
{
    error = null;
    var s = sasOrConn.Trim();
    try
    {
        if (!s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            // Connection string (may embed a SAS or an account key).
            if (string.IsNullOrWhiteSpace(containerName))
            {
                error = "a connection string was provided but no --container. Add --container <name>.";
                return null;
            }
            return new BlobContainerClient(s, containerName);
        }

        var uri = new Uri(s);
        var path = uri.AbsolutePath.Trim('/');

        if (path.Length == 0)
        {
            // Service/account SAS URL (e.g. the Portal's "Blob service SAS URL").
            if (string.IsNullOrWhiteSpace(containerName))
            {
                error = "a blob service SAS URL has no container in its path; add --container <name>, " +
                        "or use a container-scoped SAS URL instead.";
                return null;
            }
            return new BlobServiceClient(uri).GetBlobContainerClient(containerName);
        }

        // Container-scoped SAS URL (path is the container name).
        if (path.Contains('/'))
            Console.Error.WriteLine(
                $"WARN: SAS URL path has multiple segments ('{path}'); treating the full path as the container. " +
                "If this is a blob-level SAS, generate a container-scoped one instead.");
        if (!string.IsNullOrWhiteSpace(containerName) &&
            !string.Equals(path, containerName, StringComparison.OrdinalIgnoreCase))
            Console.Error.WriteLine(
                $"WARN: --container '{containerName}' differs from the container in the SAS URL ('{path}'); using the URL's container.");
        return new BlobContainerClient(uri);
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return null;
    }
}

// Builds a client for the exact blob name. These blobs carry a historical empty
// leading path segment (name starts with '/'), so the real URL has a double
// slash after the container ('.../events//Category/...'). BlobContainerClient
// .GetBlobClient(name) normalizes the leading slash away and 404s, so when we
// have the SAS URL we compose the request URL by hand to preserve it.
static BlobClient BlobClientFor(BlobContainerClient container, string credential, string blobName)
{
    if (credential.StartsWith("http", StringComparison.OrdinalIgnoreCase))
    {
        var q = credential.IndexOf('?');
        var basePart = (q >= 0 ? credential[..q] : credential).TrimEnd('/');
        var query = q >= 0 ? credential[q..] : "";
        // basePart ends at the container; blobName starts with '/', so
        // basePart + "/" + blobName yields the needed double slash.
        return new BlobClient(new Uri(basePart + "/" + blobName + query, UriKind.Absolute));
    }
    return container.GetBlobClient(blobName);
}

static string EntryBand(int count) => count switch
{
    0 => "0",
    1 => "1",
    _ => "many"
};

static string SafeFileName(string blobName)
{
    var sb = new StringBuilder();
    foreach (var ch in blobName)
    {
        if (ch is '/' or '\\') sb.Append("__");
        else if (Array.IndexOf(Path.GetInvalidFileNameChars(), ch) >= 0) sb.Append('_');
        else sb.Append(ch);
    }
    var s = sb.ToString();
    // Guard against overly long names: prefix a short stable hash, keep the tail.
    if (s.Length > 120)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(blobName)))[..8];
        s = hash + "__" + s[^100..];
    }
    if (!s.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) s += ".xml";
    return s;
}

static string Csv(string value)
{
    if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0) return value;
    return "\"" + value.Replace("\"", "\"\"") + "\"";
}

// ---- types ------------------------------------------------------------------

internal sealed record Candidate(string Name, DateTimeOffset? LastModified, long? Length, string Reason);

internal static class Feed
{
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";

    internal sealed record Info(string ContentType, string LinkRels, int EntryCount);

    internal static Info? TryParse(byte[] bytes)
    {
        XDocument doc;
        try
        {
            using var ms = new MemoryStream(bytes);
            doc = XDocument.Load(ms); // respects the XML declaration's encoding + BOM
        }
        catch
        {
            return null;
        }

        var feed = doc.Root;
        if (feed is null || feed.Name != Atom + "feed")
            return null;

        var rels = feed.Elements(Atom + "link")
            .Select(l => (string?)l.Attribute("rel"))
            .Where(r => !string.IsNullOrEmpty(r))
            .Select(r => r!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(r => r, StringComparer.Ordinal)
            .ToList();
        var relSig = rels.Count == 0 ? "(none)" : string.Join("+", rels);

        var entries = feed.Elements(Atom + "entry").ToList();
        var contentTypes = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            var child = entry.Element(Atom + "content")?.Elements().FirstOrDefault();
            if (child is not null)
                contentTypes.Add($"{{{child.Name.NamespaceName}}}{child.Name.LocalName}");
        }
        var contentKey = contentTypes.Count == 0 ? "(empty)" : string.Join(",", contentTypes);

        return new Info(contentKey, relSig, entries.Count);
    }
}

internal sealed class Options
{
    public const long MaxBlobBytes = 8L * 1024 * 1024;

    public List<string> Prefixes { get; } = new();
    public string OutDir { get; private set; } = "samples";
    public string? SasFile { get; private set; }
    public string? Container { get; private set; }
    public int PerBucket { get; private set; } = 5;
    public int Extremes { get; private set; } = 3;
    public int MaxDownloads { get; private set; } = 2000;
    public int ListCap { get; private set; } = 500_000;
    public bool Probe { get; private set; }
    public int ProbeTake { get; private set; } = 50;

    public static Options? Parse(string[] args)
    {
        var o = new Options();
        for (var i = 0; i < args.Length; i++)
        {
            string Next(string name) =>
                i + 1 < args.Length ? args[++i]
                : throw new ArgumentException($"{name} requires a value");

            try
            {
                switch (args[i])
                {
                    case "--prefix": o.Prefixes.Add(Next("--prefix").TrimEnd('/') + "/"); break;
                    case "--out": o.OutDir = Next("--out"); break;
                    case "--sas-file": o.SasFile = Next("--sas-file"); break;
                    case "--container": o.Container = Next("--container"); break;
                    case "--per-bucket": o.PerBucket = int.Parse(Next("--per-bucket")); break;
                    case "--extremes": o.Extremes = int.Parse(Next("--extremes")); break;
                    case "--max-downloads": o.MaxDownloads = int.Parse(Next("--max-downloads")); break;
                    case "--list-cap": o.ListCap = int.Parse(Next("--list-cap")); break;
                    case "--probe": o.Probe = true; break;
                    case "--probe-take": o.ProbeTake = int.Parse(Next("--probe-take")); break;
                    case "-h" or "--help": return null;
                    default:
                        Console.Error.WriteLine($"Unknown argument: {args[i]}");
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Argument error: {ex.Message}");
                return null;
            }
        }

        // No prefix => scan the whole container (warn; the point is to dig into
        // the known sub-paths, so this is usually a mistake).
        if (o.Prefixes.Count == 0)
        {
            if (!o.Probe)
                Console.Error.WriteLine("WARN: no --prefix given; scanning the whole container root.");
            o.Prefixes.Add("");
        }
        return o;
    }

    public static void PrintUsage()
    {
        Console.WriteLine(
@"AtomEventStore stratified sample extractor

Usage:
  dotnet run -- --prefix <subpath> [--prefix <subpath> ...] [options]

SAS / credential (one of):
  --sas-file <path>      File containing the SAS/connection string (gitignored).
  ATOM_SAMPLER_SAS_URL   Env var with the same, if --sas-file is omitted.

  Accepted shapes (read + list / 'rl'):
    * container-scoped SAS URL  (.../<container>?sv=...)  - self-contained, preferred
    * blob service SAS URL      (.../?sv=...)             - needs --container
    * connection string         (BlobEndpoint=...;...)    - needs --container

Options:
  --container <name>     Container name. Required for service-SAS-URL / connection-string
                         formats; ignored for a container-scoped SAS URL.
  --prefix <subpath>     A well-known sub-path to dig into (repeatable). Listing
                         recurses beneath it. Omit to scan the container root.
  --out <dir>            Output directory for samples + manifest.csv (default: samples).
  --per-bucket <n>       Max samples kept per (content-type x shape) bucket (default: 5).
  --extremes <n>         Earliest/latest blobs to always keep, per run (default: 3).
  --max-downloads <n>    Download budget cap (default: 2000).
  --list-cap <n>         Max blobs listed per prefix (default: 500000).
  --probe                Discovery mode: list raw blob names + a first-segments
                         tally (no downloads), then exit. Use to confirm the exact
                         on-the-wire prefixes (e.g. leading-slash convention).
  --probe-take <n>       Raw names to print per prefix in --probe mode (default: 50).
  -h, --help             Show this help.

Notes:
  * Only blobs ending in .xml that parse as Atom feeds are sampled.
  * Output files are real production payloads - review/scrub before committing.
");
    }
}
