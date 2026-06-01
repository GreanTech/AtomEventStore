module Grean.AtomEventStore.UnitTests.Async.FSharp.GoldenMasterTests

open System
open System.Collections.Generic
open System.IO
open System.Threading
open System.Threading.Tasks
open System.Xml
open System.Xml.Linq
open Xunit
open Swensen.Unquote
open Grean.AtomEventStore

// =============================================================================
// Validates the library's read path against the golden-master corpus: real,
// scrubbed production Atom feeds (the <content> payloads were emptied, so the
// consumer's event types aren't needed - we read the Atom *envelope* through a
// pass-through serializer). The `chain/` fixtures are one real multi-page event
// stream, so they exercise the linked-list traversal end to end.
// =============================================================================

let private atom = XNamespace.Get "http://www.w3.org/2005/Atom"

/// A serializer that doesn't need the concrete event types: it captures each
/// <content> child verbatim as an XElement and writes it back unchanged. This
/// is exactly the "library-level, payload-agnostic" approach from PLAN.md.
type PassThroughContentSerializer() =
    interface IContentSerializer with
        member _.Serialize(xmlWriter: XmlWriter, value: obj) =
            (value :?> XElement).WriteTo xmlWriter
        member _.Deserialize(xmlReader: XmlReader) =
            xmlReader.MoveToContent() |> ignore
            let element = XNode.ReadFrom xmlReader :?> XElement
            XmlAtomContent(box element)

/// Read-only storage seeded from fixture files, keyed by each feed's self-link
/// href. Implements both the sync and async storage contracts (the async one
/// trivially wraps the sync read, since there's no real I/O over local files
/// already in memory).
type FixtureStorage(feedsByHref: IDictionary<string, string>) =
    let readerFor (href: Uri) =
        match feedsByHref.TryGetValue(href.ToString()) with
        | true, xml -> XmlReader.Create(new StringReader(xml))
        | _ -> AtomEventStorage.CreateNewFeed href

    interface IAtomEventStorage with
        member _.CreateFeedReaderFor(href) = readerFor href
        member _.CreateFeedWriterFor(_) =
            raise (NotSupportedException "FixtureStorage is read-only.")

    interface IAsyncAtomEventStorage with
        member _.CreateFeedReaderForAsync(href, _) = Task.FromResult(readerFor href)
        member _.WriteFeedAsync(_, _, _) =
            raise (NotSupportedException "FixtureStorage is read-only.")

// --- fixture loading ---------------------------------------------------------

let private chainDir =
    Path.Combine(AppContext.BaseDirectory, "Fixtures", "golden-master", "chain")

// Note: in F#, `string anXAttribute` calls ToString() (markup like rel="self"),
// not the value. Always read the attribute's .Value.
let private attr (name: string) (el: XElement) =
    match el.Attribute(XName.Get name) with
    | null -> None
    | a -> Some a.Value

let private selfHref (doc: XDocument) =
    doc.Root.Elements(atom + "link")
    |> Seq.find (fun l -> attr "rel" l = Some "self")
    |> fun l -> (attr "href" l |> Option.get)

let private entryCount (doc: XDocument) =
    doc.Root.Elements(atom + "entry") |> Seq.length

/// Loads the chain fixtures, returning the parsed XDocuments and a storage
/// seeded with raw XML keyed by self-href.
let private loadChain () =
    let docs =
        Directory.EnumerateFiles(chainDir, "*.xml")
        |> Seq.map (fun path -> path, XDocument.Load path)
        |> Seq.toList
    let byHref =
        docs
        |> List.map (fun (path, doc) -> selfHref doc, File.ReadAllText path)
        |> dict
    docs |> List.map snd, FixtureStorage byHref

/// The stream id is the index page: its self-href has two equal GUID segments
/// (id/id).
let private streamIdOf (docs: XDocument list) : UuidIri =
    let isIndex (doc: XDocument) =
        match (selfHref doc).Split('/') with
        | [| a; b |] -> a = b
        | _ -> false
    let indexHref = docs |> List.find isIndex |> selfHref
    UuidIri(Guid.Parse((indexHref.Split('/')).[0]))

// --- tests -------------------------------------------------------------------

[<Fact>]
let ``Chain fixtures form a coherent linked list`` () =
    let docs, _ = loadChain ()
    let bySelf = docs |> List.map (fun d -> selfHref d, d) |> dict
    let rels (d: XDocument) =
        d.Root.Elements(atom + "link") |> Seq.choose (attr "rel") |> Set.ofSeq
    let hrefOf (d: XDocument) rel =
        d.Root.Elements(atom + "link")
        |> Seq.tryFind (fun l -> attr "rel" l = Some rel)
        |> Option.bind (attr "href")

    // Exactly one index (first+last), one first (next, no previous),
    // one last (previous, no next), and >=1 middle (next+previous).
    let has r d = rels d |> Set.contains r
    let index  = docs |> List.filter (fun d -> has "first" d && has "last" d)
    let firsts = docs |> List.filter (fun d -> has "next" d && not (has "previous" d))
    let lasts  = docs |> List.filter (fun d -> has "previous" d && not (has "next" d))
    let middles = docs |> List.filter (fun d -> has "next" d && has "previous" d)
    test <@ index.Length = 1 @>
    test <@ firsts.Length = 1 @>
    test <@ lasts.Length = 1 @>
    test <@ middles.Length >= 1 @>

    // Every next/previous/first/last href resolves to a page in the set.
    let allTargets =
        docs
        |> List.collect (fun d ->
            [ "next"; "previous"; "first"; "last" ] |> List.choose (hrefOf d))
    test <@ allTargets |> List.forall bySelf.ContainsKey @>

[<Fact>]
let ``AsyncFifoEvents traverses the entire real multi-page chain`` () : Task =
    task {
        let docs, storage = loadChain ()
        let streamId = streamIdOf docs

        // Independent expectation: total entries across every content page
        // (i.e. every page that is not the empty index).
        let isIndex (d: XDocument) =
            match (selfHref d).Split('/') with
            | [| a; b |] -> a = b
            | _ -> false
        let expectedCount =
            docs |> List.filter (isIndex >> not) |> List.sumBy entryCount

        let serializer = PassThroughContentSerializer() :> IContentSerializer
        let reader = AsyncFifoEvents<XElement>(streamId, storage, serializer)

        let actual = ResizeArray<XElement>()
        let e = reader.GetAsyncEnumerator(CancellationToken.None)
        let mutable go = true
        while go do
            let! moved = e.MoveNextAsync()
            if moved then actual.Add e.Current else go <- false
        do! e.DisposeAsync().AsTask()

        // Walking index -> first -> middle... -> last must visit every event.
        test <@ actual.Count = expectedCount @>
        // And the payload-agnostic read still surfaces the event types.
        test <@ actual |> Seq.forall (fun el -> el.Name.LocalName.StartsWith "changeset-of-") @>
    }
