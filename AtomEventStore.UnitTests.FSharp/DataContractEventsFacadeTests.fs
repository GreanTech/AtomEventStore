module Grean.AtomEventStore.UnitTests.FSharp.DataContractEventsFacadeTests

open System
open Grean.AtomEventStore
open Grean.AtomEventStore.UnitTests.FSharp.DataContractRecords
open Grean.AtomEventStore.UnitTests.FSharp.TestDsl
open Ploeh.AutoFixture
open Xunit.Extensions

[<Theory; InMemoryDataContractConventions>]
let SutCorrectlyRoundTripsASingleElement
    (writer : AtomEventObserver<TestEventF>)
    (reader : FifoEvents<TestEventF>)
    (tef : TestEventF) =

    writer.AppendAsync(tef).Wait()
    let actual = reader |> Seq.toList

    Verify <@ actual.Length = 1 @>
    Verify <@ actual |> Seq.exactlyOne = tef @>

[<Theory; InMemoryDataContractConventions>]
let SutCorrectlyRoundTripsMultipleElements
    (writer : AtomEventObserver<TestEventF>)
    (reader : FifoEvents<TestEventF>)
    (g : Generator<TestEventF>) =

    let tefs = g |> Seq.take 3 |> Seq.toList

    tefs |> List.iter (fun tef -> writer.AppendAsync(tef).Wait())
    let actual = reader |> Seq.toList

    let expected = tefs
    Verify <@ expected = actual @>

[<Theory; InMemoryDataContractConventions>]
let SutCorrectlyRoundTripsChangesetOfDiscriminatedUnion
    (writer : AtomEventObserver<Changeset<TestEvent>>)
    (reader : FifoEvents<Changeset<TestEvent>>)
    (tef : TestEventF)
    (teg : TestEventG)
    (id : Guid) =

    let expected = { Id = id; Items = [| tef |> F; teg |> G |] }

    writer.AppendAsync(expected).Wait()
    let actual = reader |> Seq.toList

    Verify <@ actual.Length = 1 @>
    Verify <@ expected = (actual |> Seq.exactlyOne) @>

[<Theory; InMemoryDataContractConventions>]
let SutCorrectlyRoundTripsMultipleChangesetOfDiscriminatedUnion
    (writer : AtomEventObserver<Changeset<TestEvent>>)
    (reader : FifoEvents<Changeset<TestEvent>>)
    (tef1 : TestEventF)
    (tef2 : TestEventF)
    (teg1 : TestEventG)
    (teg2 : TestEventG)
    (id1 : Guid)
    (id2 : Guid) =

    let changeset1 = { Id = id1; Items = [| tef1 |> F; teg1 |> G |] }
    let changeset2 = { Id = id2; Items = [| teg2 |> G; tef2 |> F |] }

    writer.AppendAsync(changeset1).Wait()
    writer.AppendAsync(changeset2).Wait()
    let actual = reader |> Seq.toList

    let expected = [changeset1; changeset2]
    Verify <@ expected = actual @>