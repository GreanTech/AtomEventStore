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
                    typeof<TestRecordsResolver>))

type InMemoryCustomization() =
    inherit CompositeCustomization(
        AtomStorageInMemoryCustomization(),
        XmlContentSerializerCustomization(),
        TestRecordsResolverCustomization())

type InMemoryConventions() =
    inherit AutoDataAttribute(Fixture().Customize(InMemoryCustomization()))

module TestDsl =
    let Verify = Swensen.Unquote.Assertions.test