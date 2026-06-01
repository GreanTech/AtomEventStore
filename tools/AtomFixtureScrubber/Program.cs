using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

// =============================================================================
// Atom fixture scrubber
//
// Reads extracted production Atom-feed blobs and writes PII-free fixtures:
//   * the Atom envelope (feed/entry ids, titles, timestamps, author, links) is
//     preserved verbatim - it carries no PII (ids/hrefs are random GUIDs, author
//     is constant, titles are "Changeset {guid}");
//   * each <content> keeps its type attribute and is reduced to a single EMPTY
//     child with the SAME qualified name as the payload root, so the content type
//     discriminator survives while the payload (the only place PII lives) is gone;
//   * the OUTPUT FILENAME is rebuilt from PII-free parts only - the category plus
//     the GUID stream/page ids from the in-XML <self> href - so identity-provider
//     ids that appear in blob PATHS (e.g. /Users/auth0_xxx/...) never reach a
//     committed fixture name.
//
// Two verification gates: (a) no text/attributes remain under any <content>;
// (b) the <self> href is pure GUIDs (else the file is name-hashed and flagged).
// Files failing (a) are reported and NOT written.
// =============================================================================

var inDir = "../AtomSampleExtractor/samples";
var outDir = "fixtures";
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--in" when i + 1 < args.Length: inDir = args[++i]; break;
        case "--out" when i + 1 < args.Length: outDir = args[++i]; break;
        case "-h" or "--help":
            Console.WriteLine("Usage: dotnet run -- [--in <dir>] [--out <dir>]");
            return 0;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            return 1;
    }
}

if (!Directory.Exists(inDir))
{
    Console.Error.WriteLine($"ERROR: input directory not found: {Path.GetFullPath(inDir)}");
    return 1;
}
Directory.CreateDirectory(outDir);

XNamespace atom = "http://www.w3.org/2005/Atom";

var files = Directory.EnumerateFiles(inDir, "*.xml").ToList();
Console.WriteLine($"Input : {Path.GetFullPath(inDir)} ({files.Count} .xml files)");
Console.WriteLine($"Output: {Path.GetFullPath(outDir)}");
Console.WriteLine();

int written = 0, payloadsStripped = 0, parseFailed = 0, verifyFailed = 0, nameHashed = 0;
var qnames = new SortedSet<string>(StringComparer.Ordinal);
var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var roleTally = new Dictionary<string, int>(StringComparer.Ordinal);

foreach (var file in files)
{
    XDocument doc;
    try
    {
        doc = XDocument.Load(file, LoadOptions.None);
    }
    catch (Exception ex)
    {
        parseFailed++;
        Console.Error.WriteLine($"WARN: skipped (not XML): {Path.GetFileName(file)} - {ex.Message}");
        continue;
    }

    foreach (var content in doc.Descendants(atom + "content").ToList())
    {
        var payload = content.Elements().FirstOrDefault();
        if (payload is not null)
        {
            qnames.Add($"{{{payload.Name.NamespaceName}}}{payload.Name.LocalName}");
            content.ReplaceNodes(new XElement(payload.Name)); // strip payload, keep QName
            payloadsStripped++;
        }
        else
        {
            content.Nodes().OfType<XText>().Remove();
        }
    }

    // Gate (a): nothing PII-bearing remains under any <content>.
    var clean = doc.Descendants(atom + "content").All(c =>
        !c.DescendantNodes().OfType<XText>().Any() &&
        c.Elements().All(e => !e.HasAttributes && !e.HasElements && e.Value.Length == 0));
    if (!clean)
    {
        verifyFailed++;
        Console.Error.WriteLine($"VERIFY FAILED (not written): {Path.GetFileName(file)}");
        continue;
    }

    // Gate (b): name the fixture from PII-free XML only - structural role + entry
    // band + the page's GUID id. Self-documenting and identifier-free.
    var (name, role, hashed) = FixtureName(doc, atom);
    if (hashed) nameHashed++;
    roleTally[role] = roleTally.GetValueOrDefault(role) + 1;

    var finalName = name;
    var dedupe = 1;
    while (!usedNames.Add(finalName))
        finalName = $"{Path.GetFileNameWithoutExtension(name)}_{++dedupe}.xml";

    doc.Save(Path.Combine(outDir, finalName));
    written++;
}

Console.WriteLine("==== Summary ====");
Console.WriteLine($"Files written        : {written} / {files.Count}");
Console.WriteLine($"Content payloads stripped: {payloadsStripped}");
Console.WriteLine($"Parse-skipped        : {parseFailed}");
Console.WriteLine($"Verify-failed (content): {verifyFailed}");
Console.WriteLine($"Name-hashed (non-GUID page id): {nameHashed}");
Console.WriteLine();
Console.WriteLine("Structural roles (fixture name prefix -> count):");
foreach (var kv in roleTally.OrderByDescending(k => k.Value))
    Console.WriteLine($"  {kv.Value,4}  {kv.Key}");
Console.WriteLine();
Console.WriteLine($"Distinct content types preserved: {qnames.Count}");
foreach (var q in qnames)
    Console.WriteLine($"  {q}");
Console.WriteLine();
if (nameHashed > 0)
    Console.Error.WriteLine(
        $"NOTE: {nameHashed} file(s) had a non-GUID <self> href and were name-hashed. " +
        "Inspect those feeds - a non-GUID href could itself carry identifying data inside the XML.");
Console.WriteLine(verifyFailed == 0
    ? "All written fixtures verified PII-free under <content>, with PII-free filenames. Review the envelope, then commit."
    : "Some files failed content verification and were NOT written - inspect before trusting the set.");
return verifyFailed == 0 ? 0 : 2;

// Names a fixture from PII-free XML only: "{role}-{band}__{pageGuid}.xml".
//   role: index | first | middle | last | single  (from the feed's link rels)
//   band: n0 | n1 | nmany                          (entry count)
//   pageGuid: the feed <id> GUID (a random identifier, not PII)
// Returns (name, role, hashed) - hashed=true if the <id> wasn't a clean GUID.
static (string name, string role, bool hashed) FixtureName(XDocument doc, XNamespace atom)
{
    var feed = doc.Root;
    var rels = feed!.Elements(atom + "link")
        .Select(l => (string?)l.Attribute("rel"))
        .Where(r => r is not null).Select(r => r!)
        .ToHashSet(StringComparer.Ordinal);

    var role =
        rels.Contains("first") && rels.Contains("last") ? "index" :
        rels.Contains("next") && rels.Contains("previous") ? "middle" :
        rels.Contains("next") ? "first" :
        rels.Contains("previous") ? "last" :
        "single";

    var entries = feed.Elements(atom + "entry").Count();
    var band = entries == 0 ? "n0" : entries == 1 ? "n1" : "nmany";

    var id = ((string?)feed.Element(atom + "id") ?? "").Replace("urn:uuid:", "");
    var hashed = !Guid.TryParse(id, out _);
    if (hashed)
        id = "h" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(id)))[..12];

    return ($"{role}-{band}__{id}.xml", role, hashed);
}
