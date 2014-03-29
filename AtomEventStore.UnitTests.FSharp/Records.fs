namespace Grean.AtomEventStore.UnitTests.FSharp

open System
open Grean.AtomEventStore

module XmlRecords =    
    open System.Xml.Serialization

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

    [<CLIMutable>]
    [<XmlRoot("changeset", Namespace = "http://grean.dk/atom-event-store/test/2014")>]
    type SerializableChangeset = {
        [<XmlElement("id")>]
        Id : Guid
        [<XmlArray("items")>]
        [<XmlArrayItem("test-event-f", typeof<TestEventF>)>]
        [<XmlArrayItem("test-event-g", typeof<TestEventG>)>]
        Items : obj array }

    type TestEvent =
        | F of TestEventF
        | G of TestEventG

    type EventChangeset = {
        Id : Guid
        Items : TestEvent seq }

    exception UnknownTypeRequested of string * string

    type TestRecordsResolver() =
        interface ITypeResolver with
            member this.Resolve(localName, xmlNamespace) =
                match (localName, xmlNamespace) with
                | ("test-event-f", "http://grean.dk/atom-event-store/test/2014") ->
                    typeof<TestEventF>
                | ("test-event-g", "http://grean.dk/atom-event-store/test/2014") ->
                    typeof<TestEventG>
                | ("changeset", "http://grean.dk/atom-event-store/test/2014") ->
                    typeof<SerializableChangeset>
                | _ -> raise(UnknownTypeRequested(localName, xmlNamespace))