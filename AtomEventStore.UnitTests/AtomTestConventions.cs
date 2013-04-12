using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomTestConventions : CompositeCustomization
    {
        public AtomTestConventions()
            : base(
                new WorkingDirectoryCustomization(),
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
    }
}
