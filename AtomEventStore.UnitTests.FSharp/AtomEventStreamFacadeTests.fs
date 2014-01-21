namespace Grean.AtomEventStore.UnitTests.FSharp

open Grean.AtomEventStore
open Grean.AtomEventStore.UnitTests.FSharp.TestDsl
open Xunit.Extensions

module AtomeEventStreamFacadeTests =

    [<Theory; InMemoryConventions>]
    let SutCorrectlyRoundTripsASingleElement
        (sut : AtomEventStream<TestEventF>)
        (tef : TestEventF) =
        
        sut.AppendAsync(tef).Wait()
        let actual = sut |> Seq.toList

        Verify <@ actual.Length = 1 @>