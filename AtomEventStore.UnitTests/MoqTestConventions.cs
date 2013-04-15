using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Grean.AtomEventStore.UnitTests
{
    public class MoqTestConventions : CompositeCustomization
    {
        public MoqTestConventions()
            : base(
                new WorkingDirectoryCustomization(),
                new SyndicationCustomization(),
                new AutoMoqCustomization())
        {
        }

        private class WorkingDirectoryCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Register(
                    () => new DirectoryInfo(Environment.CurrentDirectory));
            }
        }

        private class SyndicationCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customize<SyndicationItem>(
                    c => c.Without(x => x.SourceFeed));
            }
        }
    }
}
