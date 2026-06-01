# AtomEventStore Modernization Plan (v2.0.0)

Status: **proposed — pending review**

This plan captures the decisions reached while stress-testing the migration of
AtomEventStore from .NET Framework to modern .NET. The guiding constraint is
**do not disrupt existing consumers**: the modern version is an *additive*,
non-breaking evolution of the published `Grean.AtomEventStore` package, not a
rewrite.

> Note: this supersedes an earlier exploration that assumed a breaking "new
> replaces old" v2. Those assumptions were discarded once it became clear there
> are external consumers who must not be broken. Only the decisions below are
> authoritative.

---

## Strategy & scope

- **Additive, non-breaking modernization of the existing `Grean.AtomEventStore`
  package.** Add an async API *alongside* the untouched synchronous one. No
  public API is removed.
- **Legacy `AtomEventStore.AzureBlob`**: retired from the modern solution. Its
  already-published package remains on NuGet for laggards; it is no longer built
  in-repo.
- **New `AtomEventStore.AzureBlobAsync`**: a **reference/sample** implementation
  (not a supported package) in its **own solution**, project-referencing the
  core, demonstrating the async interfaces against the latest
  **`Azure.Storage.Blobs` (v12)** SDK.

## Target framework & compatibility

- **`netstandard2.0`-only** for both the core library and `AzureBlobAsync`.
- **net461 floor accepted.** Retargeting from net45 to netstandard2.0 raises the
  minimum .NET Framework consumer from 4.5 to 4.6.1. net45–net46.0 consumers are
  dropped; given net45 dates to 2012 and net461 to 2016, the practical impact is
  treated as negligible.
- **Wire-format compatibility is essentially free.** The async path reuses the
  existing `IContentSerializer` / `DataContractContentSerializer` code, so
  serialized output is unchanged. The persisted format in use is
  **DataContract** (not XmlSerializer).
- Historical-data read-compat is each *consumer's* concern on their own runtime,
  not a library gate. The test suite should still prove `DataContractSerializer`
  round-trips correctly on modern .NET.
- **The F# DU risk does not apply to real data.** The consuming product
  *unfolds* DU cases before serializing, so production blobs contain plain
  DataContract **record** shapes, not the fragile `[KnownType]` / DU polymorphic
  wrapper. That removes the single highest cross-runtime risk from real data.
  Plain `[DataContract]` / `[DataMember]` records round-trip reliably across
  .NET Framework → modern .NET. DU support remains a **library regression-test**
  concern only (the existing F# `Records.fs` tests), not a production
  read-compat risk.

## Async API design

- **Purely additive parallel family.** New types only:
  - `IAsyncAtomEventStorage`
  - `AsyncAtomEventObserver`
  - `AsyncFifoEvents` / `AsyncLifoEvents`
  - The existing `AtomEventsInMemory` / `AtomEventsInFiles` backends *also*
    implement `IAsyncAtomEventStorage` (one class implementing both sync and
    async interfaces).
- **Deprecate the fake-async member.** Mark `AtomEventObserver.AppendAsync`
  (the `Task.Factory.StartNew`-over-sync-IO method) `[Obsolete]` at
  **warning level** (not `error`), pointing callers to `AsyncAtomEventObserver`.
  Do **not** obsolete `OnNext` (it is legitimately synchronous). The internal
  `OnNext` → `AppendAsync().Wait()` call site gets a local suppression.
- **Async at the I/O boundary only.** Fetch bytes asynchronously → parse and
  deserialize synchronously over the buffer; serialize synchronously to a buffer
  → write the bytes asynchronously. Rationale: `DataContractSerializer` /
  `XmlSerializer` expose no async APIs, so "async all the way down" would be
  cosmetic. This keeps the serialization core untouched and the wire format
  identical.
- **Write shape: content-passing (F#-ergonomic).**
  - Reads: `Task<XmlReader> CreateFeedReaderForAsync(Uri href, CancellationToken)`
    — `XmlReader` is synchronously disposable, fine in F#.
  - Writes: `Task WriteFeedAsync(AtomFeed feed, <rendered content>, CancellationToken)`
    — a plain `Task`-returning method. The async observer renders the feed to a
    buffer synchronously (via the existing `AtomFeed.WriteTo` + serializer) and
    hands the **bytes** to storage; storage uses only `feed.Links` to decide
    where they land. Storage stays **serializer-agnostic**.
  - This deliberately avoids `IAsyncDisposable` / `await using`, which F#
    consumes awkwardly. Chosen because the codebase is F#-heavy (F# DU events,
    the F# test suite).
- **Dependency:** pull `Microsoft.Bcl.AsyncInterfaces` for `IAsyncEnumerable<T>`
  on the async read side. It is a tiny, ubiquitous Microsoft package; it becomes
  a new transitive dependency for all consumers (including sync-only), which is
  acceptable under a 2.0 release.
- **Defaults (assumed, flag if wrong):** `CancellationToken` on every async
  method; `AsyncFifoEvents` / `AsyncLifoEvents` mirror the sync next-page
  **prefetch** optimization using `await`.

## Azure (`AtomEventStore.AzureBlobAsync`)

- **Reference/sample implementation**, not a supported NuGet package. Own
  solution; **project reference** to the core (not a package reference).
- Implements `IAsyncAtomEventStorage` and exposes stream-id enumeration as
  `IAsyncEnumerable<UuidIri>` (the async counterpart to the legacy
  `IEnumerable<UuidIri>`).
- Built on **`Azure.Storage.Blobs` v12**:
  - Constructor injects a `BlobContainerClient`; authentication is the
    consumer's responsibility (mirrors the legacy "inject the container"
    pattern).
  - Listing: `GetBlobsByHierarchyAsync(delimiter: "/")` → prefix items → parse
    `Guid`.
  - Content type `application/xml` set via `BlobHttpHeaders` on upload.

## Repo & build structure

- **In-place SDK-style conversion**, reorganized into `src/` and `test/` via
  `git mv` to preserve history.
- Retire `packages.config`, the `Build-Release.ps1` / `BuildRelease.msbuild` /
  `build-release.sh` scripts, and the unused `System.ServiceModel` reference.

## Tests

- **Modernize the existing C# suite in place:**
  - xUnit 1.9.2 → **xUnit 2** (with `AutoFixture.Xunit2`). xUnit 2 is chosen
    over 3 because AutoFixture's official `[AutoData]` / `[InlineAutoData]`
    integration targets xUnit 2; xUnit 3 has no official AutoFixture adapter and
    would require a hand-maintained `DataAttribute` bridge for the AutoData-heavy
    suite.
  - AutoFixture 3.16.3 → **AutoFixture 4** (drop the `Ploeh.` namespace); port
    the customizations (`AtomEventsCustomization` etc.) to the v4 API
    (`AutoFixture` namespaces, `AutoDataAttribute(Func<IFixture>)`,
    parameterless `NoSpecimen()`).
- **Comparison strategy: C# `record` projections + xUnit `Assert`.** Replace
  SemanticComparison's `OfLikeness<T>()` and the hand-rolled `*Likeness` classes
  by projecting the type-under-test into a positional C# `record` (value
  equality; `with` expressions cover the "one member overridden" cases). This
  retires SemanticComparison *by technique*, uses one comparison idiom shared
  with the F# tests, and **avoids FluentAssertions** (whose v8 went commercial;
  only v7 is free).
- **Idioms / Albedo:** keep **`AutoFixture.Idioms` v4** (the guard-clause /
  property tests; maintained, works from a net8 C# test project); **drop
  Albedo** (`Ploeh.Albedo`, an unmaintained net35 library) — rewrite its few
  reflection conventions as hand-rolled reflection or omit them.
- **New async tests in F#**, leveraging structural equality and DU scenarios
  where they add the most value.
- Test projects target a concrete TFM (e.g. **net8.0**), not netstandard.

## Packaging, versioning & quality gate

- **Major version bump → 2.0.0** (from the current published **1.7.4**). The API
  is additive (nothing removed), but dropping net45 (floor → net461) is a
  consumer-visible compatibility change that a major version exists to signal.
  net461+ consumers upgrade cleanly; net45 holdouts are correctly warned off.
  Release notes: *"minimum framework now 4.6.1; async APIs added; no API
  removed."* **No dual-branch maintenance** of a net45 1.7.x line.
- **Quality gate:** enable the in-box **.NET Roslyn analyzers**
  (`EnableNETAnalyzers`, a chosen `AnalysisLevel`), port the old
  `AtomEventStore.ruleset` intent into **`.editorconfig`** severities, keep
  **`TreatWarningsAsErrors`**. Budget a one-time cleanup pass; prune stale
  `[SuppressMessage]` attributes and `CodeAnalysisDictionary.xml`.
- **Defaults (assumed, flag if wrong):** keep XML-doc generation
  (`GenerateDocumentationFile`) — it is a public package; use **GitHub Actions**
  with `dotnet build` / `test` / `pack` (the repo is OSS with LICENSE +
  CONTRIBUTING; the legacy scripts are retired).

---

## Open items to confirm before starting

1. ~~Real latest published version.~~ **Resolved:** the latest published version
   on NuGet.org is **1.7.4** (matches the assembly version `1.7.4.0`; the in-repo
   nuspec's `0.4.0` is stale). 2.0.0 follows directly from 1.7.4. The stale
   nuspec becomes moot once packaging moves into the SDK-style `.csproj`.
2. **Golden-master corpus (right-sized).** A production store of several GB /
   millions of blobs is available to sample from. Do **not** commit it
   wholesale — **stratify** and pull a handful per bucket:
   - **by content type** — group by the `<content>` child's qualified name (the
     unfolded case types);
   - **by structural shape** — index feed vs first/middle/last page; which link
     rels are present; single-entry vs full page; empty/edge cases;
   - **by temporal extremes** — earliest + latest blobs, to catch serialization
     *schema drift* over the product's history (renamed/added/removed
     `DataMember`s, namespace changes). This is where the large history earns its
     keep.

   Yields a compact, read-only fixture set (tens–low hundreds of files).
   **No DU samples are needed** (see the Compatibility note: the product unfolds
   DUs before serializing).

   Two concerns, owned in two places:
   - *Library-level (this repo):* validate Atom **envelope** parsing
     (feed/entry/links/content wrapper) + multi-page linked-list traversal +
     encoding on the new runtime. Can run against production blobs via a
     **pass-through serializer that captures raw content XML**, so the
     consumer's payload types are not required.
   - *Consumer-level (the product's repo):* deserializing the concrete
     DataContract event types needs their types + their blobs — belongs in the
     consuming product's own test suite, not here.

   A small **stratified sampling/extraction tool** (given read access to the
   container: bucket by content-type + structural features + time, pull N per
   bucket) is the practical way to produce this corpus rather than hand-picking
   from millions of blobs.

## Suggested sequencing

1. SDK conversion + `netstandard2.0` retarget of the core (already proven to
   compile unchanged) → green build.
2. Modernize the C# test suite (xUnit 2 / AutoFixture 4 / record projections)
   → restore regression coverage on the synchronous core.
3. Add the async family (interfaces, backends, observer, readers) + F# async
   tests.
4. Stand up `AtomEventStore.AzureBlobAsync` (separate solution, Azure SDK v12)
   as the reference implementation + its tests.
5. Wire CI (GitHub Actions), the analyzer gate, and `dotnet pack`; cut 2.0.0.

---

## Decision log (for traceability)

| Decision | Choice |
| --- | --- |
| Modernization strategy | Additive async in existing package; separate AzureBlobAsync sample; retire legacy AzureBlob |
| Persisted serializer in use | DataContractContentSerializer |
| Target framework | netstandard2.0-only (both projects); net461 floor accepted |
| Async surface | Additive parallel family + `[Obsolete]` (warning) on fake-async `AppendAsync` |
| Async depth | I/O boundary only; serialization stays synchronous |
| Async storage write shape | Content-passing `WriteFeedAsync(feed, content, ct)`; reads `Task<XmlReader>`; F#-ergonomic, no `IAsyncDisposable` |
| Azure adapter status | Reference/sample (unpublished); own solution; project reference; Azure.Storage.Blobs v12 |
| Repo structure | In-place SDK conversion; `src/`+`test/` via `git mv`; retire old build infra |
| Test strategy | Modernize C# in place + new async tests in F# |
| xUnit version | xUnit 2 (+ AutoFixture.Xunit2) |
| Comparison approach | C# `record` projections + xUnit `Assert` (no FluentAssertions) |
| Idioms / Albedo | Keep AutoFixture.Idioms v4; drop Albedo |
| Versioning | Major bump 1.7.4 → 2.0.0; no dual-branch maintenance |
| Quality gate | Roslyn analyzers + `.editorconfig` + `TreatWarningsAsErrors` |
