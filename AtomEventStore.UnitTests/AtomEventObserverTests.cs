using Ploeh.AutoFixture.Idioms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Ploeh.AutoFixture;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventObserverTests
    {
        [Theory, AutoAtomData]
        public void PropertiesAreCorrectlyInitialized(
            ConstructorInitializedMemberAssertion assertion)
        {
            assertion.Verify(typeof(AtomEventObserver<TestEventX>));
        }

        [Theory, AutoAtomData]
        public void AppendAsyncCorrectlyStoresFeed(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            XmlAttributedTestEventX expectedEvent)
        {
            var before = DateTimeOffset.Now;

            sut.AppendAsync(expectedEvent).Wait();

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var actual = FindFirstPage(writtenFeeds, sut.Id);
            var expectedFeed =
                new AtomFeedLikeness(before, actual.Id, expectedEvent);
            Assert.True(
                expectedFeed.Equals(actual),
                "Expected feed must match actual feed.");
        }

        [Theory, AutoAtomData]
        public void AppendAsyncFirstEventWritesPageBeforeIndex(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]SpyAtomEventStore spyStore,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            XmlAttributedTestEventX @event)
        {
            sut.AppendAsync(@event).Wait();

            var feed = Assert.IsAssignableFrom<AtomFeed>(
                spyStore.ObservedArguments.Last());
            Assert.Equal(sut.Id, feed.Id);
        }

        [Theory, AutoAtomData]
        public void AppendAsyncTwoEventsOnlyWritesIndexOnce(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]SpyAtomEventStore spyStore,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var events = eventGenerator.Take(2).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenIds = spyStore.ObservedArguments
                .OfType<AtomFeed>()
                .Select(f => f.Id);
            Assert.Equal(1, writtenIds.Count(id => sut.Id == id));
        }

        [Theory, AutoAtomData]
        public void AppendAsyncPageSizeEventsStoresAllEntriesInFirstPage(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var actual = FindFirstPage(writtenFeeds, sut.Id);
            var expectedFeed = new AtomFeedLikeness(
                before,
                actual.Id,
                events.AsEnumerable().Reverse().ToArray());
            Assert.True(
                expectedFeed.Equals(actual),
                "Expected feed must match actual feed.");
        }

        [Theory, AutoAtomData]
        public void AppendAsyncMoreThanPageSizeEventsOnlyStoresOverflowingEvent(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextPage = FindNextPage(firstPage, writtenFeeds);
            var expectedPage = new AtomFeedLikeness(
                before,
                nextPage.Id,
                events.AsEnumerable().Reverse().First());
            Assert.True(
                expectedPage.Equals(nextPage),
                "Expected feed must match actual feed.");
        }

        [Theory, AutoAtomData]
        public void AppendAsyncMoreThanPageSizeEventsWritesInCorrectOrder(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]SpyAtomEventStore spyStore,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenFeeds = spyStore.Feeds.Select(ParseAtomFeed);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextPage = FindNextPage(firstPage, writtenFeeds);
            var expected = new[] { nextPage.Id, firstPage.Id, sut.Id };
            var actual = spyStore
                .ObservedArguments
                .OfType<AtomFeed>()
                .Select(f => f.Id)
                .Reverse()
                .Take(3)
                .Reverse();
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void AppendAsyncMoreThanPageSizeEventsOnlyWritesIndexTwice(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]SpyAtomEventStore spyStore,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenIds = spyStore.ObservedArguments
                .OfType<AtomFeed>()
                .Select(f => f.Id);
            Assert.Equal(2, writtenIds.Count(id => sut.Id == id));
        }

        [Theory, AutoAtomData]
        public void AppendAsyncCorrectlyStoresLastLinkOnIndex(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            XmlAttributedTestEventX expectedEvent)
        {
            var before = DateTimeOffset.Now;

            sut.AppendAsync(expectedEvent).Wait();

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var index = FindIndex(writtenFeeds, sut.Id);
            var firstLink = index.Links.SingleOrDefault(l => l.IsFirstLink);
            Assert.NotNull(firstLink);
            var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
            Assert.NotNull(lastLink);
            Assert.Equal(firstLink.Href, lastLink.Href);
        }

        [Theory, AutoAtomData]
        public void AppendAsyncMoreThanPageSizeEventsCorrectlyUpdatesLastLink(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var index = FindIndex(writtenFeeds, sut.Id);
            var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
            Assert.NotNull(lastLink);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextPage = FindNextPage(firstPage, writtenFeeds);
            var expected = nextPage.Links.SingleOrDefault(l => l.IsSelfLink);
            Assert.NotNull(expected);
            Assert.Equal(expected.Href, lastLink.Href);
        }

        [Theory, AutoAtomData]
        public void AppendAsyncMultipeTimesMoreThanPageSizeCorrectlyStoresOverflowingEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize * 2).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextPage = FindNextPage(firstPage, writtenFeeds);
            var expectedPage = new AtomFeedLikeness(
                before,
                nextPage.Id,
                events.Skip(sut.PageSize).Reverse().ToArray());
            Assert.True(
                expectedPage.Equals(nextPage),
                "Expected feed must match actual feed.");
        }

        [Theory, AutoAtomData]
        public void AppendAsyncMoreThanTwicePageSizeCorrectlyStoresOverflowingEvent(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize * 2 + 1).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextPage = FindNextPage(firstPage, writtenFeeds);
            nextPage = FindNextPage(nextPage, writtenFeeds);
            var expectedPage = new AtomFeedLikeness(
                before,
                nextPage.Id,
                events.AsEnumerable().Reverse().First());
            Assert.True(
                expectedPage.Equals(nextPage),
                "Expected feed must match actual feed.");
        }

        [Theory, AutoAtomData]
        public void AppendAsyncWritesEventToCorrectPageEvenIfLastLinkIsNotUpToDate(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            // Fixture setup
            var events = eventGenerator.Take(sut.PageSize * 2 + 1).ToList();
            events.ForEach(e => sut.AppendAsync(e).Wait());

            /* Point the 'last' link to the second page, instead of to the last
             * page. This simulates that when the true last page was created,
             * the index wasn't correctly updated. This could for example
             * happen due to a network failure. */
            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var index = FindIndex(writtenFeeds, sut.Id);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextPage = FindNextPage(firstPage, writtenFeeds);
            var incorrectLastLink =
                nextPage.Links.Single(l => l.IsSelfLink).ToLastLink();
            index = index.WithLinks(index.Links
                .Where(l => !l.IsLastLink)
                .Concat(new[] { incorrectLastLink }));
            using (var w = storage.CreateFeedWriterFor(index))
                index.WriteTo(w, sut.Serializer);

            var expected = eventGenerator.First();

            // Exercise system
            sut.AppendAsync(expected).Wait();

            // Verify outcome
            writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            index = FindIndex(writtenFeeds, sut.Id);
            firstPage = FindFirstPage(writtenFeeds, sut.Id);
            nextPage = FindNextPage(firstPage, writtenFeeds);
            nextPage = FindNextPage(nextPage, writtenFeeds);

            Assert.Equal(expected, nextPage.Entries.First().Content.Item);
            Assert.Equal(
                nextPage.Links.Single(l => l.IsSelfLink).Href,
                index.Links.Single(l => l.IsLastLink).Href);
            // Teardown
        }

        [Theory, AutoAtomData]
        public void AppendAsyncMoreThanPageSizeEventsAddsNextPageWithPreviousLink(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();

            events.ForEach(e => sut.AppendAsync(e).Wait());

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextPage = FindNextPage(firstPage, writtenFeeds);
            var previousLink = 
                nextPage.Links.SingleOrDefault(l => l.IsPreviousLink);
            Assert.NotNull(previousLink);
            Assert.Equal(
                firstPage.Links.Single(l => l.IsSelfLink).Href,
                previousLink.Href);
        }

        private static AtomFeed FindIndex(IEnumerable<AtomFeed> pages, UuidIri id)
        {
            var index = pages.SingleOrDefault(f => f.Id == id);
            Assert.NotNull(index);
            return index;
        }

        private static AtomFeed FindFirstPage(IEnumerable<AtomFeed> pages, UuidIri id)
        {
            var index = FindIndex(pages, id);
            var firstLink = index.Links.SingleOrDefault(l => l.IsFirstLink);
            Assert.NotNull(firstLink);
            Guid g;
            Assert.True(Guid.TryParse(firstLink.Href.ToString(), out g));
            var firstPage = pages.SingleOrDefault(f => f.Id == (UuidIri)g);
            Assert.NotNull(firstPage);
            return firstPage;
        }

        private static AtomFeed FindNextPage(AtomFeed page, IEnumerable<AtomFeed> pages)
        {
            var nextLink = page.Links.SingleOrDefault(l => l.IsNextLink);
            Assert.NotNull(nextLink);
            Guid g;
            Assert.True(Guid.TryParse(nextLink.Href.ToString(), out g));
            var nextPage = pages.SingleOrDefault(f => f.Id == (UuidIri)g);
            Assert.NotNull(nextPage);
            return nextPage;
        }

        private static AtomFeed ParseAtomFeed(string xml)
        {
            return AtomFeed.Parse(
                xml,
                new XmlContentSerializer(new TestEventTypeResolver()));
        }

        public class TestEventTypeResolver : ITypeResolver
        {
            public Type Resolve(string localName, string xmlNamespace)
            {
                switch (xmlNamespace)
                {
                    case "http://grean:rocks":
                        switch (localName)
                        {
                            case "test-event-x":
                                return typeof(XmlAttributedTestEventX);
                            case "test-event-y":
                                return typeof(XmlAttributedTestEventY);
                            default:
                                throw new ArgumentException("Unexpected local name.", "localName");
                        }
                    default:
                        throw new ArgumentException("Unexpected XML namespace.", "xmlNamespace");
                }
            }
        }
    }
}
