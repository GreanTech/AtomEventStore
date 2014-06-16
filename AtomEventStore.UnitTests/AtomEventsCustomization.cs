using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventsCustomization : CompositeCustomization
    {
        public AtomEventsCustomization()
            : base(
                new PageSizeCustomization(),
                new TypeResolverCustomization(),
                new ContentSerializerCustomization(),
                new DirectoryCustomization(),
                new StreamCustomization(),
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
                        return new NoSpecimen(request);

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
                        return new NoSpecimen(request);

                    if (pi.Member.ReflectedType == typeof(AtomEventStream<XmlAttributedTestEventX>))
                        return context.Resolve(typeof(XmlContentSerializer));
                    if (pi.Member.ReflectedType == typeof(AtomEventStream<IXmlAttributedTestEvent>))
                        return context.Resolve(typeof(XmlContentSerializer));
                    if (pi.Member.ReflectedType == typeof(AtomEventStream<DataContractEnvelope<IDataContractTestEvent>>))
                        return context.Resolve(typeof(DataContractContentSerializer));

                    return new NoSpecimen(request);
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
                        return new NoSpecimen(request);

                    if (pi.Member.ReflectedType == typeof(XmlContentSerializer))
                        return new XmlContentTypeResolver();

                    if (pi.Member.ReflectedType == typeof(DataContractContentSerializer))
                        return new DataContractTypeResolver();

                    return new NoSpecimen(request);
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
    }
}
