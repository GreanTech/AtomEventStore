namespace Grean.AtomEventStore.UnitTests.FSharp

open System.Xml.Serialization
open Grean.AtomEventStore

[<CLIMutable>]
[<XmlRoot("test-event-f", Namespace = "http://grean.dk/atom-event-store/test/2014")>]
type TestEventF = {
    [<XmlElement("number")>]
    Number : int
    [<XmlElement("text")>]
    Text : string }

[<CLIMutable>]
[<XmlRoot("test-event-g", Namespace = "http://grean.dk/atom-event-store/test/2014")>]
type TestEventG = {
    [<XmlElement("number")>]
    Number : byte
    [<XmlElement("flag")>]
    Flag : bool }

type TestEvent =
    | F of TestEventF
    | G of TestEventG

exception UnknownTypeRequested of string * string

type TestRecordsResolver() =
    interface ITypeResolver with
        member this.Resolve(localName, xmlNamespace) =
            match (localName, xmlNamespace) with
            | ("test-event-f", "http://grean.dk/atom-event-store/test/2014") ->
                typeof<TestEventF>
            | ("test-event-g", "http://grean.dk/atom-event-store/test/2014") ->
                typeof<TestEventG>
            | _ -> raise(UnknownTypeRequested(localName, xmlNamespace))