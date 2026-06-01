using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventsCustomization : CompositeCustomization
    {
        public AtomEventsCustomization()
            : base(
                new PageSizeCustomization(),
                new TypeResolverCustomization(),
                new ReadOnlyCollectionCustomization(),
                new ContentSerializerCustomization(),
                new DirectoryCustomization(),
                new StreamCustomization(),
                new XmlIoCustomization(),
                new AutoMoqCustomization())
        {
        }

        private class PageSizeCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customizations.Add(new PageSizeRelay());
            }

            private class PageSizeRelay : ISpecimenBuilder
            {
                private readonly Random r = new Random();

                public object Create(object request, ISpecimenContext context)
                {
                    var pi = request as ParameterInfo;
                    if (pi == null ||
                        pi.ParameterType != typeof(int) ||
                        pi.Name != "pageSize")
                        return new NoSpecimen();

                    return this.r.Next(2, 17);
                }
            }
        }

        private class ContentSerializerCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customizations.Add(new ContentSerializerBuilder());
            }

            private class ContentSerializerBuilder : ISpecimenBuilder
            {
                public object Create(object request, ISpecimenContext context)
                {
                    var pi = request as ParameterInfo;
                    if (pi == null || pi.ParameterType != typeof(IContentSerializer))
                        return new NoSpecimen();

#pragma warning disable 618
                    if (pi.Member.ReflectedType == typeof(AtomEventStream<XmlAttributedTestEventX>))
                        return context.Resolve(typeof(XmlContentSerializer));
                    if (pi.Member.ReflectedType == typeof(AtomEventStream<IXmlAttributedTestEvent>))
                        return context.Resolve(typeof(XmlContentSerializer));
                    if (pi.Member.ReflectedType == typeof(AtomEventStream<DataContractEnvelope<IDataContractTestEvent>>))
                        return context.Resolve(typeof(DataContractContentSerializer));
#pragma warning restore 618

                    return new NoSpecimen();
                }
            }
        }

        private class TypeResolverCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customizations.Add(new TypeResolverBuilder());
            }

            private class TypeResolverBuilder : ISpecimenBuilder
            {
                public object Create(object request, ISpecimenContext context)
                {
                    var pi = request as ParameterInfo;
                    if (pi == null || pi.ParameterType != typeof(ITypeResolver))
                        return new NoSpecimen();

                    if (pi.Member.ReflectedType == typeof(XmlContentSerializer))
                        return new XmlContentTypeResolver();

                    if (pi.Member.ReflectedType == typeof(DataContractContentSerializer))
                        return new DataContractTypeResolver();

                    return new NoSpecimen();
                }

                private class XmlContentTypeResolver : ITypeResolver
                {
                    public Type Resolve(string localName, string xmlNamespace)
                    {
                        switch (xmlNamespace)
                        {
                            case "http://grean:rocks":
                                switch (localName)
                                {
                                    case "test-event-x":
                                        return typeof(XmlAttributedTestEventX);
                                    case "test-event-y":
                                        return typeof(XmlAttributedTestEventY);
                                    case "changeset":
                                        return typeof(XmlAttributedChangeset);
                                    default:
                                        throw new ArgumentException("Unexpected local name: " + localName, "localName");
                                }
                            default:
                                throw new ArgumentException("Unexpected XML namespace: " + xmlNamespace, "xmlNamespace");
                        }
                    }
                }

                private class DataContractTypeResolver : ITypeResolver
                {
                    public Type Resolve(string localName, string xmlNamespace)
                    {
                        switch (xmlNamespace)
                        {
                            case "http://grean.rocks/dc":
                                switch (localName)
                                {
                                    case "envelope":
                                        return typeof(DataContractEnvelope<IDataContractTestEvent>);
                                    case "test-event-x":
                                        return typeof(DataContractTestEventX);
                                    default:
                                        throw new ArgumentException("Unexpected local name: " + localName, "localName");
                                }
                            default:
                                throw new ArgumentException("Unexpected XML namespace: " + xmlNamespace, "xmlNamespace");
                        }
                    }
                }

            }
        }

        private class DirectoryCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Inject(
                    new DirectoryInfo(Environment.CurrentDirectory));
            }
        }

        private class StreamCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Register<Stream>(
                    () => new MemoryStream());
            }
        }

        // XmlReader/XmlWriter are abstract, so AutoFixture can't create them on
        // its own; supply real instances so guard-clause verification of the
        // ReadFrom/WriteTo methods can exercise their parameters.
        private class XmlIoCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Register<XmlReader>(
                    () => XmlReader.Create(new StringReader("<root />")));
                fixture.Register<XmlWriter>(
                    () => XmlWriter.Create(new MemoryStream()));
            }
        }

        private class ReadOnlyCollectionCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customizations.Add(new ReadOnlyCollectionRelay());
            }

            private class ReadOnlyCollectionRelay : ISpecimenBuilder
            {
                public object Create(object request, ISpecimenContext context)
                {
                    if (context == null)
                        throw new ArgumentNullException("context");

                    var type = request as Type;
                    if (type == null)
                        return new NoSpecimen();

                    var args = type.GetGenericArguments();
                    if (args.Length != 1 ||
                        type.GetGenericTypeDefinition() !=
                            typeof(IReadOnlyCollection<>))
                        return new NoSpecimen();

                    var relayedType =
                        typeof(ICollection<>).MakeGenericType(args);
                    return context.Resolve(relayedType);
                }
            }
        }
    }
}
