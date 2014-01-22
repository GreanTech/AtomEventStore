namespace Grean.AtomEventStore.UnitTests.FSharp

open Grean.AtomEventStore
open Grean.AtomEventStore.UnitTests.FSharp.TestDsl
open Ploeh.AutoFixture
open Xunit.Extensions

module AtomeEventStreamFacadeTests =

    [<Theory; InMemoryConventions>]
    let SutCorrectlyRoundTripsASingleElement
        (sut : AtomEventStream<TestEventF>)
        (tef : TestEventF) =
        
        sut.AppendAsync(tef).Wait()
        let actual = sut |> Seq.toList

        Verify <@ actual.Length = 1 @>

    [<Theory; InMemoryConventions>]
    let SutCurrectlyRoundTripsMultipleElements
        (sut : AtomEventStream<TestEventF>)
        (g : Generator<TestEventF>) =

        let tefs = g |> Seq.take 3 |> Seq.toList

        tefs |> List.iter(fun tef -> sut.AppendAsync(tef).Wait())
        let actual = sut |> Seq.toList

        let expected = tefs |> List.rev
        Verify <@ expected = actual @>