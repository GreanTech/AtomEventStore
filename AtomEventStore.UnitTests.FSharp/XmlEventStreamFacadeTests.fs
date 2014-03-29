namespace Grean.AtomEventStore.UnitTests.FSharp

open System
open System.Reactive
open FSharp.Reactive
open Grean.AtomEventStore
open Grean.AtomEventStore.UnitTests.FSharp.TestDsl
open Ploeh.AutoFixture
open Xunit.Extensions

module XmlEventStreamFacadeTests =

    [<Theory; InMemoryXmlConventions>]
    let SutCorrectlyRoundTripsASingleElement
        (sut : AtomEventStream<TestEventF>)
        (tef : TestEventF) =
        
        sut.AppendAsync(tef).Wait()
        let actual = sut |> Seq.toList

        Verify <@ actual.Length = 1 @>

    [<Theory; InMemoryXmlConventions>]
    let SutCorrectlyRoundTripsMultipleElements
        (sut : AtomEventStream<TestEventF>)
        (g : Generator<TestEventF>) =

        let tefs = g |> Seq.take 3 |> Seq.toList

        tefs |> List.iter(fun tef -> sut.AppendAsync(tef).Wait())
        let actual = sut |> Seq.toList

        let expected = tefs |> List.rev
        Verify <@ expected = actual @>

    [<Theory; InMemoryXmlConventions>]
    let SutCorrectlyRoundTripsDiscriminatedUnions
        (sut : AtomEventStream<obj>)
        (tef : TestEventF)
        (teg : TestEventG) =

        let extract = function
            | F(x) -> x :> obj
            | G(x) -> x :> obj
        let duObs = Observer.Create(extract >> sut.OnNext)
        duObs.OnNext(tef |> F)
        duObs.OnNext(teg |> G)
        
        let infuse (x : obj) =
            match x with
            | :? TestEventF as f -> f |> F
            | :? TestEventG as g -> g |> G
            | _ -> raise(System.ArgumentException("Unknown event type."))
        let duSeq = sut |> Seq.map infuse
        let actual = duSeq |> Seq.toList

        let expected = [teg |> G; tef |> F]
        Verify <@ expected = actual @>

    [<Theory; InMemoryXmlConventions>]
    let SutCorrectlyRoundTripsChangesetOfDiscriminatedUnions
        (sut : AtomEventStream<SerializableChangeset>)
        (tef : TestEventF)
        (teg : TestEventG)
        (id : Guid) =

        let toObj changeset =
            let extract = function
                | F(x) -> x :> obj
                | G(x) -> x :> obj
            let events = changeset.Items |> Seq.map extract |> Seq.toArray
            { SerializableChangeset.Id = changeset.Id; Items = events }
        let duObs = Observer.Create(toObj >> sut.OnNext)
        duObs.OnNext({ Id = id; Items = [| tef |> F; teg |> G |]})

        let ofObj (changeset : SerializableChangeset) =
            let infuse (x : obj) =
                match x with
                | :? TestEventF as f -> f |> F
                | :? TestEventG as g -> g |> G
                | _ -> raise(System.ArgumentException("Unknown event type."))
            let events = changeset.Items |> Array.map infuse
            { Id = changeset.Id; Items = events }
        let duSeq = sut |> Seq.map ofObj
        let actual = duSeq |> Seq.toList

        let expected = [ { Id = id; Items = [| tef |> F; teg |> G |] } ]
        Verify <@ expected = actual @>