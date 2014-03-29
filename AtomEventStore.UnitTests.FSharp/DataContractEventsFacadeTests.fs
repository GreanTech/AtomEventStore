module Grean.AtomEventStore.UnitTests.FSharp.DataContractEventsFacadeTests

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