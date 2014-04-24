namespace Grean.AtomEventStore.UnitTests.FSharp

open System.Reflection
open Grean.AtomEventStore
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Kernel
open Ploeh.AutoFixture.Xunit

type AtomStorageInMemoryCustomization() =
    let storage = new AtomEventsInMemory()
    interface ICustomization with
        member this.Customize fixture =
            fixture.Inject<IAtomEventStorage> storage

type XmlContentSerializerCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customizations.Add(
                TypeRelay(
                    typeof<IContentSerializer>,
                    typeof<XmlContentSerializer>))

type XmlTestRecordsResolverCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customizations.Add(
                TypeRelay(
                    typeof<ITypeResolver>,
                    typeof<XmlRecords.TestRecordsResolver>))

type DataContractContentSerializerCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customizations.Add(
                TypeRelay(
                    typeof<IContentSerializer>,
                    typeof<DataContractContentSerializer>))

type DataContracTestRecordsResolverCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customizations.Add(
                TypeRelay(
                    typeof<ITypeResolver>,
                    typeof<DataContractRecords.TestRecordsResolver>))

type InMemoryXmlCustomization() =
    inherit CompositeCustomization(
        AtomStorageInMemoryCustomization(),
        XmlContentSerializerCustomization(),
        XmlTestRecordsResolverCustomization())

type InMemoryXmlConventions() =
    inherit AutoDataAttribute(Fixture().Customize(InMemoryXmlCustomization()))

type FrozenEventsIdCustomization() =
    let freezeEventsId id = {
        new ISpecimenBuilder with
            member this.Create(request, context) =
                match request with
                | :? ParameterInfo as param
                    when param.ParameterType = typeof<UuidIri>
                    && param.Name = "id" -> id
                | _ -> NoSpecimen(request) :> obj }
    interface ICustomization with
        member this.Customize fixture =
            let id = fixture.Create<UuidIri>()
            fixture.Customizations.Add(freezeEventsId id)

type InMemoryDataContractCustomization() =
    inherit CompositeCustomization(
        FrozenEventsIdCustomization(),
        AtomStorageInMemoryCustomization(),
        DataContractContentSerializerCustomization(),
        DataContracTestRecordsResolverCustomization())

type InMemoryDataContractConventions() =
    inherit AutoDataAttribute(Fixture().Customize(InMemoryDataContractCustomization()))

module TestDsl =
    let Verify = Swensen.Unquote.Assertions.test