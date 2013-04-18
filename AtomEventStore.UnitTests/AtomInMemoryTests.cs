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
            AtomEntryBuilder<TestEventX> entry)
        {
            var expected = entry.Build();

            using (var w = sut.CreateWriterFor(expected))
                expected.WriteTo(w);
            using (var r = sut.CreateReaderFor(expected.Id))
            {
                var actual = AtomEntry.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCanReadFirstEntry(
            AtomInMemory sut,
            AtomEntryBuilder<TestEventX> entry1,
            AtomEntryBuilder<TestEventY> entry2)
        {
            var expected = entry1.Build();
            var other = entry2.Build();

            using (var w = sut.CreateWriterFor(expected))
                expected.WriteTo(w);
            using (var w = sut.CreateWriterFor(other))
                other.WriteTo(w);

            using (var r = sut.CreateReaderFor(expected.Id))
            {
                var actual = AtomEntry.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCanReadSecondEntry(
            AtomInMemory sut,
            AtomEntryBuilder<TestEventX> entry1,
            AtomEntryBuilder<TestEventY> entry2)
        {
            var other = entry1.Build();
            var expected = entry2.Build();

            using (var w = sut.CreateWriterFor(other))
                other.WriteTo(w);
            using (var w = sut.CreateWriterFor(expected))
                expected.WriteTo(w);

            using (var r = sut.CreateReaderFor(expected.Id))
            {
                var actual = AtomEntry.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCannotRepeatedWriteSameEntry(
            AtomInMemory sut,
            AtomEntryBuilder<TestEventX> entryBuilder)
        {
            var entry = entryBuilder.Build();
            using (var w = sut.CreateWriterFor(entry))
                entry.WriteTo(w);

            Assert.Throws<InvalidOperationException>(
                () => sut.CreateWriterFor(entry));
        }
    }
}
