using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace Grean.AtomEventStore.UnitTests
{
    public class FakeTestConventions : CompositeCustomization
    {
        public FakeTestConventions()
            : base(
                new FakesCustomization())
        {
        }

        private class FakesCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                var fake = fixture.Create<InMemorySyndication>();
                fixture.Inject<ISyndicationFeedReader>(fake);
                fixture.Inject<ISyndicationFeedWriter>(fake);
                fixture.Inject<ISyndicationItemWriter>(fake);
            }
        }
    }
}
