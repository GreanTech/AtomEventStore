using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
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
                    new DirectoryCustomization(),
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
        }
    }
}
