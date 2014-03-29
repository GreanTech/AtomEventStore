namespace Grean.AtomEventStore.UnitTests.FSharp

open Grean.AtomEventStore
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Kernel
open Ploeh.AutoFixture.Xunit

type AtomStorageInMemoryCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customizations.Add(
                TypeRelay(
                    typeof<IAtomEventStorage>,
                    typeof<AtomEventsInMemory>))

type XmlContentSerializerCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customizations.Add(
                TypeRelay(
                    typeof<IContentSerializer>,
                    typeof<XmlContentSerializer>))

type TestRecordsResolverCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customizations.Add(
                TypeRelay(
                    typeof<ITypeResolver>,
                    typeof<XmlRecords.TestRecordsResolver>))

type InMemoryXmlCustomization() =
    inherit CompositeCustomization(
        AtomStorageInMemoryCustomization(),
        XmlContentSerializerCustomization(),
        TestRecordsResolverCustomization())

type InMemoryXmlConventions() =
    inherit AutoDataAttribute(Fixture().Customize(InMemoryXmlCustomization()))

module TestDsl =
    let Verify = Swensen.Unquote.Assertions.test