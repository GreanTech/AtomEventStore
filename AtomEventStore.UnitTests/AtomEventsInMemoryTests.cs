using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Moq;
using Ploeh.AutoFixture;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventsInMemoryTests
    {
        [Theory, AutoAtomData]
        public void ClientCanReadWrittenFeed(
            AtomEventsInMemory sut,
            AtomFeedBuilder<TestEventX> feedBuilder)
        {
            var expected = feedBuilder.Build();

            using (var w = sut.CreateFeedWriterFor(expected))
                expected.WriteTo(
                    w,
                    new ConventionBasedSerializerOfComplexImmutableClasses());
            using (var r = sut.CreateFeedReaderFor(expected.Locate()))
            {
                var actual = AtomFeed.ReadFrom(
                    r,
                    new ConventionBasedSerializerOfComplexImmutableClasses());

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
                expected.WriteTo(
                    w,
                    new ConventionBasedSerializerOfComplexImmutableClasses());
            using (var w = sut.CreateFeedWriterFor(other))
                other.WriteTo(
                    w,
                    new ConventionBasedSerializerOfComplexImmutableClasses());

            using (var r = sut.CreateFeedReaderFor(expected.Locate()))
            {
                var actual = AtomFeed.ReadFrom(
                    r,
                    new ConventionBasedSerializerOfComplexImmutableClasses());

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
                other.WriteTo(
                    w,
                    new ConventionBasedSerializerOfComplexImmutableClasses());
            using (var w = sut.CreateFeedWriterFor(expected))
                expected.WriteTo(
                    w,
                    new ConventionBasedSerializerOfComplexImmutableClasses());

            using (var r = sut.CreateFeedReaderFor(expected.Locate()))
            {
                var actual = AtomFeed.ReadFrom(
                    r,
                    new ConventionBasedSerializerOfComplexImmutableClasses());

                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ReadNonPersistedFeedReturnsCorrectFeed(
            AtomEventsInMemory sut,
            UuidIri id,
            IContentSerializer dummySerializer)
        {
            var expectedSelfLink = AtomLink.CreateSelfLink(
                new Uri(
                    ((Guid)id).ToString(),
                    UriKind.Relative));
            var before = DateTimeOffset.Now;

            using (var r = sut.CreateFeedReaderFor(expectedSelfLink.Href))
            {
                var actual = AtomFeed.ReadFrom(r, dummySerializer);

                Assert.Equal(id, actual.Id);
                Assert.Equal("Index of event stream " + (Guid)id, actual.Title);
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

        [Theory, AutoAtomData]
        public void FeedsAreInitiallyEmpty(AtomEventsInMemory sut)
        {
            IEnumerable<string> actual = sut.Feeds;
            Assert.Empty(actual);
        }

        [Theory, AutoAtomData]
        public void FeedsReturnWrittenFeeds(
            AtomEventsInMemory sut,
            IEnumerable<AtomFeedBuilder<TestEventY>> feedBuilders)
        {
            var feeds = feedBuilders.Select(b => b.Build());
            foreach (var f in feeds)
                using (var w = sut.CreateFeedWriterFor(f))
                    f.WriteTo(
                        w,
                        new ConventionBasedSerializerOfComplexImmutableClasses());

            var actual = sut.Feeds.Select(s => AtomFeed.Parse(s, new ConventionBasedSerializerOfComplexImmutableClasses()));

            var expected = new HashSet<AtomFeed>(feeds, new AtomFeedComparer());
            Assert.True(
                expected.SetEquals(actual),
                "Written feeds should be enumerated.");
        }

        [Theory, AutoAtomData]
        public void SutIsEnumerableOfIds(AtomEventsInMemory sut)
        {
            Assert.IsAssignableFrom<IEnumerable<UuidIri>>(sut);
        }

        [Theory, AutoAtomData]
        public void EmptySutIsEmpty(AtomEventsInMemory sut)
        {
            Assert.False(sut.Any(), "Empty store should be empty.");
            Assert.Empty(sut);
        }

        [Theory, AutoAtomData]
        public void SutEnumeratesIndexedIndexes(
            AtomEventsInMemory sut,
            IEnumerable<AtomFeedBuilder<DataContractTestEventX>> feedBuilders,
            Mock<ITypeResolver> resolverStub)
        {
            resolverStub
                .Setup(r => r.Resolve("test-event-x", "http://grean.rocks/dc"))
                .Returns(typeof(DataContractTestEventX));
            var feeds = feedBuilders
                .Select(b => b.Build())
                .Select(f => f.WithLinks(f.Links.Select(MakeSelfLinkIndexed)))
                .ToArray();
            foreach (var f in feeds)
                using (var w = sut.CreateFeedWriterFor(f))
                    f.WriteTo(
                        w,
                        new DataContractContentSerializer(
                            resolverStub.Object));

            var actual = sut;

            var expected = new HashSet<UuidIri>(feeds.Select(f => f.Id));
            Assert.True(
                expected.SetEquals(actual),
                "AtomEventsInMemory should yield index IDs.");
        }

        [Theory, AutoAtomData]
        public void SutOnlyEnumeratesIndexedIndexes(
            AtomEventsInMemory sut,
            Generator<AtomFeedBuilder<DataContractTestEventX>> feedBuilders,
            Mock<ITypeResolver> resolverStub)
        {
            resolverStub
                .Setup(r => r.Resolve("test-event-x", "http://grean.rocks/dc"))
                .Returns(typeof(DataContractTestEventX));
            var feeds = feedBuilders.Select(b => b.Build()).Take(5).ToList();
            var indexes = feeds
                .PickRandom(2)
                .Select(f => f.WithLinks(f.Links.Select(MakeSelfLinkIndexed)))
                .ToList();

            var feedsToWrite = feeds
                .Where(f => !indexes.Any(i => i.Id == f.Id))
                .Concat(indexes)
                .ToArray()
                .Shuffle();
            foreach (var f in feedsToWrite)
                using (var w = sut.CreateFeedWriterFor(f))
                    f.WriteTo(
                        w,
                        new DataContractContentSerializer(
                            resolverStub.Object));

            var actual = sut;

            var expected = new HashSet<UuidIri>(indexes.Select(f => f.Id));
            Assert.True(
                expected.SetEquals(actual),
                "AtomEventsInMemory should only yield index IDs.");
        }

        /* This test attempts to verify that the SUT is thread-safe. Obviously,
         * this is a naive approach, but better than nothing. It does actually
         * reproduce a failure mode seen in the wild, at least on the
         * computer it was written. */
        [Theory, AutoAtomData]
        public void SutSeemsToBeThreadSafe(
            AtomEventsInMemory sut,
            Generator<AtomFeedBuilder<DataContractTestEventX>> g,
            Mock<ITypeResolver> resolverStub)
        {
            resolverStub
                .Setup(r => r.Resolve("test-event-x", "http://grean.rocks/dc"))
                .Returns(typeof(DataContractTestEventX));
            var feeds = g.Take(500).Select(b => b.Build()).ToArray();

            var tasks = feeds
                .AsParallel()
                .Select(f => Task.Factory.StartNew(() =>
                {
                    using (var w = sut.CreateFeedWriterFor(f))
                        f.WriteTo(
                            w,
                            new DataContractContentSerializer(
                                resolverStub.Object));
                }))
                .ToArray();
            var readFeeds = feeds
                .AsParallel()
                .Select(f => f.Locate())
                .Select(uri =>
                {
                    using (var r = sut.CreateFeedReaderFor(uri))
                        return AtomFeed.ReadFrom(
                            r,
                            new DataContractContentSerializer(
                                resolverStub.Object));
                })
                .ToArray();
            Task.WaitAll(tasks);

            Assert.Equal(feeds.Length, readFeeds.Length);
        }

        private static AtomLink MakeSelfLinkIndexed(AtomLink link)
        {
            if (link.IsSelfLink)
            {
                var segment = GetIdFromHref(link.Href);
                var indexedHref = segment + "/" + segment;
                return link.WithHref(new Uri(indexedHref, UriKind.Relative));
            }
            else
                return link;
        }

        /* This is actually the third time this piece of code has been copied,
         * so according to the Rule of Three, we should consider refactoring
         * it. However, since it's such a crappy workaround, and since it has
         * very strong preconditions, it may be of limited value making it
         * public. */
        private static Guid GetIdFromHref(Uri href)
        {
            /* The assumption here is that the href argument is always going to
             * be a relative URL. So far at least, that's consistent with how
             * AtomEventStore works.
             * However, the Segments property only works for absolute URLs. */
            var fakeBase = new Uri("http://grean.com");
            var absoluteHref = new Uri(fakeBase, href);
            // The ID is assumed to be contained in the last segment of the URL
            var lastSegment = absoluteHref.Segments.Last();
            return new Guid(lastSegment);
        }
    }
}
