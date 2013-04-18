using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomInMemoryTests
    {
        [Theory, AutoAtomData]
        public void ClientCanReadWrittenEntry(
            AtomInMemory sut,
            AtomEntry entry,
            TestEventX tex)
        {
            var expected = entry.WithContent(entry.Content.WithItem(tex));

            using (var w = sut.CreateWriterFor(expected))
                expected.WriteTo(w);
            using (var r = sut.CreateReaderFor(expected.Id))
            {
                var actual = AtomEntry.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }
    }
}
