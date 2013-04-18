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
    public class AtomEventsInFilesTests
    {
        [Theory, AutoAtomData]
        public void SutIsAtomEventStorage(AtomEventsInFiles sut)
        {
            Assert.IsAssignableFrom<IAtomEventStorage>(sut);
        }
    }
}
