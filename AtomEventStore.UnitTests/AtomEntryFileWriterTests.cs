using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEntryFileWriterTests
    {
        [Theory, AutoAtomData]
        public void SutIsSyndicationItemWriter(AtomEntryFileWriter sut)
        {
            Assert.IsAssignableFrom<ISyndicationItemWriter>(sut);
        }
    }
}
