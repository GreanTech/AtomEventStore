using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class AutoAtomDataAttribute : AutoDataAttribute
    {
        public AutoAtomDataAttribute()
            : base(new Fixture().Customize(new AtomEventsCustomization()))
        {
        }

        private class AtomEventsCustomization : CompositeCustomization
        {
            public AtomEventsCustomization()
                : base(
                    new PageSizeCustomization(),
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
                    fixture.Customizations.Add(
                        new TypeRelay(
                            typeof(IContentSerializer),
                            typeof(ConventionBasedSerializerOfComplexImmutableClasses)));
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
}
