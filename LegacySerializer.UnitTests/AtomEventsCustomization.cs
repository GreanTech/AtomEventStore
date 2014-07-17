using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    public class AtomEventsCustomization : CompositeCustomization
    {
        public AtomEventsCustomization()
            : base(
                new DirectoryCustomization(),
                new StreamCustomization(),
                new AutoMoqCustomization())
        {
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
