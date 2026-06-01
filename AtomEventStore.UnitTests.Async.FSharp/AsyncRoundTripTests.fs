module Grean.AtomEventStore.UnitTests.Async.FSharp.AsyncRoundTripTests

open System
open System.Collections.Generic
open System.IO
open System.Threading
open System.Threading.Tasks
open System.Xml.Serialization
open Xunit
open Swensen.Unquote
open Grean.AtomEventStore

// A simple XmlSerializer-friendly event. As an F# record it has structural
// equality, so round-trip assertions are a plain `expected = actual`.
[<CLIMutable>]
[<XmlRoot("test-event", Namespace = "urn:grean:async-tests")>]
type TestEvent =
    { [<XmlElement("number")>] Number : int
      [<XmlElement("text")>]   Text   : string }

let private serializer () : IContentSerializer =
    XmlContentSerializer.Scan(typeof<TestEvent>.Assembly)

// Collect an IAsyncEnumerable<'T> into an F# list (no TaskSeq dependency).
let private toListAsync (source : IAsyncEnumerable<'T>) : Task<'T list> =
    task {
        let acc = ResizeArray<'T>()
        let e = source.GetAsyncEnumerator(CancellationToken.None)
        let mutable go = true
        while go do
            let! moved = e.MoveNextAsync()
            if moved then acc.Add e.Current else go <- false
        do! e.DisposeAsync().AsTask()
        return List.ofSeq acc
    }

// --- in-memory round-trips ---------------------------------------------------

[<Fact>]
let ``AsyncFifoEvents reads back a single appended event`` () : Task =
    task {
        let id = UuidIri.NewId()
        use storage = new AtomEventsInMemory()
        let ser = serializer ()
        let writer = AsyncAtomEventObserver<TestEvent>(id, 25, storage, ser)

        let event = { Number = 42; Text = "the answer" }
        // F#-ergonomic write: a single do!, no IAsyncDisposable dance.
        do! writer.AppendAsync event

        let reader = AsyncFifoEvents<TestEvent>(id, storage, ser)
        let! actual = toListAsync reader

        test <@ [ event ] = actual @>
    }

[<Fact>]
let ``AsyncFifoEvents reads multiple events in order (multi-page)`` () : Task =
    task {
        let id = UuidIri.NewId()
        use storage = new AtomEventsInMemory()
        let ser = serializer ()
        // pageSize = 1 forces a multi-page linked list, exercising next/previous.
        let writer = AsyncAtomEventObserver<TestEvent>(id, 1, storage, ser)

        let expected =
            [ for i in 1..5 -> { Number = i; Text = sprintf "event %d" i } ]
        for e in expected do
            do! writer.AppendAsync(e, CancellationToken.None)

        let reader = AsyncFifoEvents<TestEvent>(id, storage, ser)
        let! actual = toListAsync reader

        test <@ expected = actual @>
    }

[<Fact>]
let ``AsyncLifoEvents reads multiple events in reverse`` () : Task =
    task {
        let id = UuidIri.NewId()
        use storage = new AtomEventsInMemory()
        let ser = serializer ()
        let writer = AsyncAtomEventObserver<TestEvent>(id, 1, storage, ser)

        let appended =
            [ for i in 1..4 -> { Number = i; Text = sprintf "event %d" i } ]
        for e in appended do
            do! writer.AppendAsync e

        let reader = AsyncLifoEvents<TestEvent>(id, storage, ser)
        let! actual = toListAsync reader

        test <@ (List.rev appended) = actual @>
    }

// --- file-backed round-trip (real async I/O) --------------------------------

[<Fact>]
let ``AsyncFifoEvents round-trips through file storage`` () : Task =
    task {
        let id = UuidIri.NewId()
        let dir =
            Path.Combine(
                Path.GetTempPath(),
                "AtomEventStore-async-" + string (Guid.NewGuid()))
        let storage = AtomEventsInFiles(DirectoryInfo dir)
        let ser = serializer ()
        let writer = AsyncAtomEventObserver<TestEvent>(id, 2, storage, ser)

        let expected =
            [ for i in 1..3 -> { Number = i; Text = sprintf "persisted %d" i } ]
        try
            for e in expected do
                do! writer.AppendAsync e

            let reader = AsyncFifoEvents<TestEvent>(id, storage, ser)
            let! actual = toListAsync reader

            test <@ expected = actual @>
        finally
            if Directory.Exists dir then Directory.Delete(dir, true)
    }
