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
    public class AtomEventsInMemoryTests
    {
        [Theory, AutoAtomData]
        public void ClientCanReadWrittenEntry(
            AtomEventsInMemory sut,
            AtomEntryBuilder<TestEventX> entry)
        {
            var expected = entry.Build();

            using (var w = sut.CreateEntryWriterFor(expected))
                expected.WriteTo(w);
            using (var r = sut.CreateEntryReaderFor(expected.Locate()))
            {
                var actual = AtomEntry.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCanReadFirstEntry(
            AtomEventsInMemory sut,
            AtomEntryBuilder<TestEventX> entry1,
            AtomEntryBuilder<TestEventY> entry2)
        {
            var expected = entry1.Build();
            var other = entry2.Build();

            using (var w = sut.CreateEntryWriterFor(expected))
                expected.WriteTo(w);
            using (var w = sut.CreateEntryWriterFor(other))
                other.WriteTo(w);

            using (var r = sut.CreateEntryReaderFor(expected.Locate()))
            {
                var actual = AtomEntry.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCanReadSecondEntry(
            AtomEventsInMemory sut,
            AtomEntryBuilder<TestEventX> entry1,
            AtomEntryBuilder<TestEventY> entry2)
        {
            var other = entry1.Build();
            var expected = entry2.Build();

            using (var w = sut.CreateEntryWriterFor(other))
                other.WriteTo(w);
            using (var w = sut.CreateEntryWriterFor(expected))
                expected.WriteTo(w);

            using (var r = sut.CreateEntryReaderFor(expected.Locate()))
            {
                var actual = AtomEntry.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCannotRepeatedWriteSameEntry(
            AtomEventsInMemory sut,
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
            AtomEventsInMemory sut,
            AtomFeedBuilder<TestEventX> feedBuilder)
        {
            var expected = feedBuilder.Build();

            using (var w = sut.CreateFeedWriterFor(expected))
                expected.WriteTo(w);
            using (var r = sut.CreateFeedReaderFor(expected.Locate()))
            {
                var actual = AtomFeed.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCanReadFirstFeed(
            AtomEventsInMemory sut,
            AtomFeedBuilder<TestEventX> feedBuilder1,
            AtomFeedBuilder<TestEventY> feedBuilder2)
        {
            var expected = feedBuilder1.Build();
            var other = feedBuilder2.Build();

            using (var w = sut.CreateFeedWriterFor(expected))
                expected.WriteTo(w);
            using (var w = sut.CreateFeedWriterFor(other))
                other.WriteTo(w);

            using (var r = sut.CreateFeedReaderFor(expected.Locate()))
            {
                var actual = AtomFeed.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ClientCanReadSecondFeed(
            AtomEventsInMemory sut,
            AtomFeedBuilder<TestEventX> feedBuilder1,
            AtomFeedBuilder<TestEventY> feedBuilder2)
        {
            var other = feedBuilder1.Build();
            var expected = feedBuilder2.Build();

            using (var w = sut.CreateFeedWriterFor(other))
                other.WriteTo(w);
            using (var w = sut.CreateFeedWriterFor(expected))
                expected.WriteTo(w);

            using (var r = sut.CreateFeedReaderFor(expected.Locate()))
            {
                var actual = AtomFeed.ReadFrom(r);

                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ReadNonPersistedFeedReturnsCorrectFeed(
            AtomEventsInMemory sut,
            UuidIri id)
        {
            var expectedSelfLink = AtomLink.CreateSelfLink(
                new Uri(
                    ((Guid)id).ToString(),
                    UriKind.Relative));
            var before = DateTimeOffset.Now;

            using (var r = sut.CreateFeedReaderFor(expectedSelfLink.Href))
            {
                var actual = AtomFeed.ReadFrom(r);

                Assert.Equal(id, actual.Id);
                Assert.Equal("Head of event stream " + (Guid)id, actual.Title);
                Assert.True(before <= actual.Updated, "Updated should be very recent.");
                Assert.True(actual.Updated <= DateTimeOffset.Now, "Updated should not be in the future.");
                Assert.Empty(actual.Entries);
                Assert.Contains(
                    expectedSelfLink,
                    actual.Links);
            }
        }

        [Theory, AutoAtomData]
        public void SutIsAtomEventPersistence(AtomEventsInMemory sut)
        {
            Assert.IsAssignableFrom<IAtomEventStorage>(sut);
        }
    }
}
