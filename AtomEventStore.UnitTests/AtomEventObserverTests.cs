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
using Moq;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventObserverTests
    {
        [Theory, AutoAtomData]
        public void PropertiesAreCorrectlyInitialized(
            ConstructorInitializedMemberAssertion assertion)
        {
            assertion.Verify(typeof(AtomEventObserver<DataContractTestEventX>));
        }

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteCorrectlyStoresFeed(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            XmlAttributedTestEventX expectedEvent)
        {
            var before = DateTimeOffset.Now;

            writerFactory.Create(usage).WriteTo(sut, expectedEvent);

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var actual = FindFirstPage(writtenFeeds, sut.Id);
            var expectedFeed =
                new AtomFeedLikeness(before, actual.Id, expectedEvent);
            Assert.True(
                expectedFeed.Equals(actual),
                "Expected feed must match actual feed.");
        }

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteFirstEventWritesPageBeforeIndex(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]SpyAtomEventStore spyStore,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            XmlAttributedTestEventX @event)
        {
            writerFactory.Create(usage).WriteTo(sut, @event);

            var feed = Assert.IsAssignableFrom<AtomFeed>(
                spyStore.ObservedArguments.Last());
            Assert.Equal(sut.Id, feed.Id);
        }

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteTwoEventsOnlyWritesIndexOnce(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]SpyAtomEventStore spyStore,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var events = eventGenerator.Take(2).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

            var writtenIds = spyStore.ObservedArguments
                .OfType<AtomFeed>()
                .Select(f => f.Id);
            Assert.Equal(1, writtenIds.Count(id => sut.Id == id));
        }

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WritePageSizeEventsStoresAllEntriesInFirstPage(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

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

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteMoreThanPageSizeEventsOnlyStoresOverflowingEvent(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

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

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteMoreThanPageSizeEventsWritesInCorrectOrder(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]SpyAtomEventStore spyStore,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

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

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteMoreThanPageSizeEventsOnlyWritesIndexTwice(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]SpyAtomEventStore spyStore,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

            var writtenIds = spyStore.ObservedArguments
                .OfType<AtomFeed>()
                .Select(f => f.Id);
            Assert.Equal(2, writtenIds.Count(id => sut.Id == id));
        }

        [Theory]
        [InlineAutoAtomData(1, AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(1, AtomEventWriteUsage.OnNext)]
        [InlineAutoAtomData(2, AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(2, AtomEventWriteUsage.OnNext)]
        public void WriteMoreThanPageSizeDoesNotThrowWhenLastWriteFails(
            int pageCount,
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen]Mock<IAtomEventStorage> storeStub,
            AtomEventsInMemory innerStorage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            // Fixture setup
            storeStub
                .Setup(s => s.CreateFeedReaderFor(It.IsAny<Uri>()))
                .Returns((Uri u) => innerStorage.CreateFeedReaderFor(u));
            storeStub
                .Setup(s => s.CreateFeedWriterFor(It.IsAny<AtomFeed>()))
                .Returns((AtomFeed f) => innerStorage.CreateFeedWriterFor(f));
            var writer = writerFactory.Create(usage);

            /* Write some pages full. */
            var events = eventGenerator.Take(sut.PageSize * pageCount).ToList();
            events.ForEach(e => writer.WriteTo(sut, e));

            /* Find the index. */
            var writtenFeeds = innerStorage.Feeds.Select(ParseAtomFeed);
            var index = FindIndex(writtenFeeds, sut.Id);

            /* Configure storage to fail when writing the index, but not for
             * other documents. */
            storeStub
                .Setup(s => s.CreateFeedWriterFor(
                    It.Is<AtomFeed>(f => f.Id != index.Id)))
                .Returns((AtomFeed f) => innerStorage.CreateFeedWriterFor(f));
            storeStub
                .Setup(s => s.CreateFeedWriterFor(
                    It.Is<AtomFeed>(f => f.Id == index.Id)))
                .Throws(new Exception("On-purpose write failure."));

            // Exercise system
            var @event = eventGenerator.First();
            writer.WriteTo(sut, @event);

            // Verify outcome
            writtenFeeds = innerStorage.Feeds.Select(ParseAtomFeed);
            var lastPage = FindLastPage(writtenFeeds, sut.Id);
            Assert.True(
                lastPage.Entries.Select(e => e.Content.Item).Any(@event.Equals),
                "Last written event should be present.");
            // Teardown
        }

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteCorrectlyStoresLastLinkOnIndex(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            XmlAttributedTestEventX expectedEvent)
        {
            var before = DateTimeOffset.Now;

            writerFactory.Create(usage).WriteTo(sut, expectedEvent);

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var index = FindIndex(writtenFeeds, sut.Id);
            var firstLink = index.Links.SingleOrDefault(l => l.IsFirstLink);
            Assert.NotNull(firstLink);
            var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
            Assert.NotNull(lastLink);
            Assert.Equal(firstLink.Href, lastLink.Href);
        }

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteMoreThanPageSizeEventsCorrectlyUpdatesLastLink(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

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

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteMultipeTimesMoreThanPageSizeCorrectlyStoresOverflowingEvents(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize * 2).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

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

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteMoreThanTwicePageSizeCorrectlyStoresOverflowingEvent(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var before = DateTimeOffset.Now;
            var events = eventGenerator.Take(sut.PageSize * 2 + 1).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

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

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteWritesEventToCorrectPageEvenIfLastLinkIsNotUpToDate(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            // Fixture setup
            var writer = writerFactory.Create(usage);
            var events = eventGenerator.Take(sut.PageSize * 2 + 1).ToList();
            events.ForEach(e => writer.WriteTo(sut, e));

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
            writer.WriteTo(sut, expected);

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

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void AttemptToCorrecLastLinkDoesNotThrowOnFailure(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen]Mock<IAtomEventStorage> storeStub,
            AtomEventsInMemory innerStorage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            // Fixture setup
            storeStub
                .Setup(s => s.CreateFeedReaderFor(It.IsAny<Uri>()))
                .Returns((Uri u) => innerStorage.CreateFeedReaderFor(u));
            storeStub
                .Setup(s => s.CreateFeedWriterFor(It.IsAny<AtomFeed>()))
                .Returns((AtomFeed f) => innerStorage.CreateFeedWriterFor(f));
            var writer = writerFactory.Create(usage);
            var events = eventGenerator.Take(sut.PageSize * 2 + 1).ToList();
            events.ForEach(e => writer.WriteTo(sut, e));

            /* Point the 'last' link to the second page, instead of to the last
             * page. This simulates that when the true last page was created,
             * the index wasn't correctly updated. This could for example
             * happen due to a network failure. */
            var writtenFeeds = innerStorage.Feeds.Select(ParseAtomFeed);
            var index = FindIndex(writtenFeeds, sut.Id);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextPage = FindNextPage(firstPage, writtenFeeds);
            var incorrectLastLink =
                nextPage.Links.Single(l => l.IsSelfLink).ToLastLink();
            index = index.WithLinks(index.Links
                .Where(l => !l.IsLastLink)
                .Concat(new[] { incorrectLastLink }));
            using (var w = innerStorage.CreateFeedWriterFor(index))
                index.WriteTo(w, sut.Serializer);

            /* Configure storage to fail when writing the index, but not for
             * other documents. */
            storeStub
                .Setup(s => s.CreateFeedWriterFor(
                    It.Is<AtomFeed>(f => f.Id != index.Id)))
                .Returns((AtomFeed f) => innerStorage.CreateFeedWriterFor(f));
            storeStub
                .Setup(s => s.CreateFeedWriterFor(
                    It.Is<AtomFeed>(f => f.Id == index.Id)))
                .Throws(new Exception("On-purpose write failure."));

            var expected = eventGenerator.First();

            // Exercise system
            writer.WriteTo(sut, expected);

            // Verify outcome
            writtenFeeds = innerStorage.Feeds.Select(ParseAtomFeed);
            var lastPage = FindLastPage(writtenFeeds, sut.Id);
            Assert.True(
                lastPage.Entries.Select(e => e.Content.Item).Any(expected.Equals),
                "Last written event should be present.");
            // Teardown
        }

        [Theory]
        [InlineAutoAtomData(AtomEventWriteUsage.AppendAsync)]
        [InlineAutoAtomData(AtomEventWriteUsage.OnNext)]
        public void WriteMoreThanPageSizeEventsAddsNextPageWithPreviousLink(
            AtomEventWriteUsage usage,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventWriterFactory<XmlAttributedTestEventX> writerFactory,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var events = eventGenerator.Take(sut.PageSize + 1).ToList();
            var writer = writerFactory.Create(usage);

            events.ForEach(e => writer.WriteTo(sut, e));

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
            var firstPage = pages.SingleOrDefault(f => f.Links.Single(l => l.IsSelfLink).Href == firstLink.Href);
            Assert.NotNull(firstPage);
            return firstPage;
        }

        private static AtomFeed FindNextPage(AtomFeed page, IEnumerable<AtomFeed> pages)
        {
            var nextLink = page.Links.SingleOrDefault(l => l.IsNextLink);
            Assert.NotNull(nextLink);
            var nextPage = pages.SingleOrDefault(f => f.Links.Single(l => l.IsSelfLink).Href == nextLink.Href);
            Assert.NotNull(nextPage);
            return nextPage;
        }

        private static AtomFeed FindLastPage(IEnumerable<AtomFeed> pages, UuidIri id)
        {
            var page = FindFirstPage(pages, id);
            while (page.Links.Any(l => l.IsNextLink))
                page = FindNextPage(page, pages);

            return page;
        }

        private static AtomFeed ParseAtomFeed(string xml)
        {
            return AtomFeed.Parse(
                xml,
                new XmlContentSerializer(new TestEventTypeResolver()));
        }

        [Theory, AutoAtomData]
        public void SutIsObserver(AtomEventObserver<DataContractTestEventX> sut)
        {
            Assert.IsAssignableFrom<IObserver<DataContractTestEventX>>(sut);
        }

        [Theory, AutoAtomData]
        public void SutHasAppropriateGuardClauses(
            GuardClauseAssertion assertion)
        {
            assertion.Verify(
                typeof(AtomEventObserver<>)
                    .GetMembers()
                    .Where(m => m.Name != "OnError"));
        }

        [Theory, AutoAtomData]
        public void OnErrorDoesNotThrow(
            AtomEventObserver<DataContractTestEventX> sut,
            Exception error)
        {
            Assert.DoesNotThrow(() => sut.OnError(error));
        }

        [Theory, AutoAtomData]
        public void OnCompletedDoesNotThrow(
            AtomEventObserver<DataContractTestEventX> sut)
        {
            Assert.DoesNotThrow(sut.OnCompleted);
        }
    }

    public interface IAtomEventWriter<T>
    {
        void WriteTo(AtomEventObserver<T> observer, T @event);
    }

    public enum AtomEventWriteUsage
    {
        AppendAsync = 0,
        OnNext
    }

    public class AppendAsyncAtomEventWriter<T> : IAtomEventWriter<T>
    {
        public void WriteTo(AtomEventObserver<T> observer, T @event)
        {
            observer.AppendAsync(@event).Wait();
        }
    }

    public class OnNextAtomEventWriter<T> : IAtomEventWriter<T>
    {
        public void WriteTo(AtomEventObserver<T> observer, T @event)
        {
            observer.OnNext(@event);
        }
    }

    public class AtomEventWriterFactory<T>
    {
        public IAtomEventWriter<T> Create(AtomEventWriteUsage use)
        {
            switch (use)
            {
                case AtomEventWriteUsage.AppendAsync:
                    return new AppendAsyncAtomEventWriter<T>();
                case AtomEventWriteUsage.OnNext:
                    return new OnNextAtomEventWriter<T>();
                default:
                    throw new ArgumentOutOfRangeException("Unexpected value.");
            }
        }
    }
}
