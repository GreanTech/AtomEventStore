# AtomSampleExtractor

A standalone utility (not part of the library) that builds the **golden-master
corpus** described in `PLAN.md` (open item #2): a compact, representative set of
real Atom-feed blobs sampled from a production container, for validating the
modern read path against what was actually persisted.

It is **stratified**, so a handful of GB / millions of blobs collapse into tens
to low hundreds of files:

- **content type** — the qualified name of the `<content>` child element;
- **structural shape** — the set of Atom link rels (`self`/`previous`/`next`/
  `first`/`last`) plus an entry-count band (`0` / `1` / `many`);
- **temporal extremes** — the earliest and latest blobs (to catch serialization
  *schema drift* over the store's history).

## Digging into sub-paths

This library implicitly expects stream blobs at the container root
(`{streamId}/{pageId}.xml`). If your account nests them under a known set of
sub-paths, pass each as a `--prefix`; listing recurses beneath it. Only blobs
ending in `.xml` that actually parse as Atom feeds are sampled, so unrelated
content under those paths is ignored. The feeds use **relative** link hrefs, so
the extracted XML is path-portable and works with the library's parser
regardless of where it lived.

## SAS handling (read-only)

The tool needs **read + list (`rl`)** access. The credential is read from a
gitignored file or an env var and is **never** printed or written to the
manifest. Three shapes are accepted:

| Portal source | Example | Needs `--container`? |
| --- | --- | --- |
| Container *Generate SAS* (preferred, least-privilege) | `https://acct.blob.core.windows.net/<container>?sv=...` | no — container is in the path |
| Account *Shared access signature* → "Blob service SAS URL" | `https://acct.blob.core.windows.net/?sv=...` | **yes** |
| Account *Shared access signature* → "Connection string" | `BlobEndpoint=...;SharedAccessSignature=sv=...` | **yes** |

```powershell
# Put the credential in a gitignored file (.sas is ignored):
Set-Content -Path .\container.sas -Value '<paste SAS URL or connection string here>'
```

## Run

```powershell
# from tools/AtomSampleExtractor
dotnet run -- `
  --sas-file .\container.sas `
  --prefix tenantA/events `
  --prefix tenantB/events `
  --out .\samples `
  --per-bucket 5
```

Or via env var instead of `--sas-file`:

```powershell
$env:ATOM_SAMPLER_SAS_URL = '<container SAS URL>'
dotnet run -- --prefix tenantA/events
```

## Output

- `samples/` — the kept blobs, written verbatim (filenames are the blob paths
  with `/` → `__`).
- `samples/manifest.csv` — one row per downloaded candidate:
  `blobName, lastModified, length, contentType, linkRels, entryCount, bucket,
  reason (earliest|latest|spread), kept`.

The run prints a summary including every bucket discovered (seen vs kept) and
explicit warnings if any cap was hit (listing truncation, download budget) — no
silent truncation.

## ⚠️ The samples are real production data

`samples/` is gitignored. **Review and scrub** (PII, secrets in payloads) before
copying any curated subset into committed test fixtures. Per `PLAN.md`, the
*library* tests only need the Atom envelope (which a pass-through serializer can
read without your payload types); deserializing the concrete DataContract event
types belongs in the consuming product's own test suite.

## Options

| Option | Default | Meaning |
| --- | --- | --- |
| `--prefix <p>` | (root) | Well-known sub-path to dig into; repeatable. |
| `--out <dir>` | `samples` | Output directory. |
| `--sas-file <path>` | — | File with the SAS URL or connection string. |
| `--container <name>` | — | Container name; required for service-SAS-URL / connection-string formats. |
| `--per-bucket <n>` | `5` | Max samples kept per bucket. |
| `--extremes <n>` | `3` | Earliest/latest blobs always kept. |
| `--max-downloads <n>` | `2000` | Download budget cap. |
| `--list-cap <n>` | `500000` | Max blobs listed per prefix. |
