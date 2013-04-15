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
                fixture.Customizations.Add(
                    new TypeRelay(
                        typeof(ISyndicationFeedReader),
                        typeof(InMemorySyndication)));
                fixture.Customizations.Add(
                    new TypeRelay(
                        typeof(ISyndicationFeedWriter),
                        typeof(InMemorySyndication)));
                fixture.Customizations.Add(
                    new TypeRelay(
                        typeof(ISyndicationItemWriter),
                        typeof(InMemorySyndication)));
            }
        }
    }
}
