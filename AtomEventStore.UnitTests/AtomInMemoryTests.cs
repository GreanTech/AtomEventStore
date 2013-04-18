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

            using (var w = sut.CreateEntryWriterFor(expected))
                expected.WriteTo(w);
            using (var r = sut.CreateEntryReaderFor(expected.Id))
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

            using (var w = sut.CreateEntryWriterFor(expected))
                expected.WriteTo(w);
            using (var w = sut.CreateEntryWriterFor(other))
                other.WriteTo(w);

            using (var r = sut.CreateEntryReaderFor(expected.Id))
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

            using (var w = sut.CreateEntryWriterFor(other))
                other.WriteTo(w);
            using (var w = sut.CreateEntryWriterFor(expected))
                expected.WriteTo(w);

            using (var r = sut.CreateEntryReaderFor(expected.Id))
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
            using (var w = sut.CreateEntryWriterFor(entry))
                entry.WriteTo(w);

            Assert.Throws<InvalidOperationException>(
                () => sut.CreateEntryWriterFor(entry));
        }

        [Theory, AutoAtomData]
        public void ClientCanReadWrittenFeed(
            AtomInMemory sut,
            AtomFeedBuilder<TestEventX> feedBuilder)
        {
            var expected = feedBuilder.Build();

            using (var w = sut.CreateFeedWriterFor(expected))
                expected.WriteTo(w);
            using (var r = sut.CreateFeedReaderFor(expected.Id))
            {
                var actual = AtomFeed.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCanReadFirstFeed(
            AtomInMemory sut,
            AtomFeedBuilder<TestEventX> feedBuilder1,
            AtomFeedBuilder<TestEventY> feedBuilder2)
        {
            var expected = feedBuilder1.Build();
            var other = feedBuilder2.Build();

            using (var w = sut.CreateFeedWriterFor(expected))
                expected.WriteTo(w);
            using (var w = sut.CreateFeedWriterFor(other))
                other.WriteTo(w);

            using (var r = sut.CreateFeedReaderFor(expected.Id))
            {
                var actual = AtomFeed.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCanReadSecondFeed(
            AtomInMemory sut,
            AtomFeedBuilder<TestEventX> feedBuilder1,
            AtomFeedBuilder<TestEventY> feedBuilder2)
        {
            var other = feedBuilder1.Build();
            var expected = feedBuilder2.Build();

            using (var w = sut.CreateFeedWriterFor(other))
                other.WriteTo(w);
            using (var w = sut.CreateFeedWriterFor(expected))
                expected.WriteTo(w);

            using (var r = sut.CreateFeedReaderFor(expected.Id))
            {
                var actual = AtomFeed.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }
    }
}
