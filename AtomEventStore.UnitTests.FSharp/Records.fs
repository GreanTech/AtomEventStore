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

exception UnknownTypeRequested of string * string

type TestRecordsResolver() =
    interface ITypeResolver with
        member this.Resolve(localName, xmlNamespace) =
            match (localName, xmlNamespace) with
            | ("test-event-f", "http://grean.dk/atom-event-store/test/2014") ->
                typeof<TestEventF>
            | _ -> raise(UnknownTypeRequested(localName, xmlNamespace))