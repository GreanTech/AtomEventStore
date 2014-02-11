﻿using Ploeh.AutoFixture.Idioms;
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
            var nextLink = firstPage.Links.SingleOrDefault(l => l.IsNextLink);
            Assert.NotNull(nextLink);
            Guid g;
            Assert.True(Guid.TryParse(nextLink.Href.ToString(), out g));
            var nextPage = writtenFeeds.SingleOrDefault(f => f.Id == (UuidIri)g);
            var expectedPage = new AtomFeedLikeness(
                before,
                nextPage.Id,
                events.AsEnumerable().Reverse().First());
            Assert.True(
                expectedPage.Equals(nextPage),
                "Expected feed must match actual feed.");
        }

        [Theory, AutoAtomData]
        public void AppendAsyncCorrectlyStoreLastLinkOnIndex(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            XmlAttributedTestEventX expectedEvent)
        {
            var before = DateTimeOffset.Now;

            sut.AppendAsync(expectedEvent).Wait();

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var index = writtenFeeds.SingleOrDefault(f => f.Id == sut.Id);
            Assert.NotNull(index);
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
            var index = writtenFeeds.SingleOrDefault(f => f.Id == sut.Id);
            Assert.NotNull(index);
            var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
            Assert.NotNull(lastLink);
            var firstPage = FindFirstPage(writtenFeeds, sut.Id);
            var nextLink = firstPage.Links.SingleOrDefault(l => l.IsNextLink);
            Assert.NotNull(nextLink);
            Guid g;
            Assert.True(Guid.TryParse(nextLink.Href.ToString(), out g));
            var nextPage = writtenFeeds.SingleOrDefault(f => f.Id == (UuidIri)g);
            var expected = nextPage.Links.SingleOrDefault(l => l.IsSelfLink);
            Assert.NotNull(expected);
            Assert.Equal(expected.Href, lastLink.Href);
        }

        private static AtomFeed FindFirstPage(IEnumerable<AtomFeed> pages, UuidIri id)
        {
            var index = pages.SingleOrDefault(f => f.Id == id);
            Assert.NotNull(index);
            var firstLink = index.Links.SingleOrDefault(l => l.IsFirstLink);
            Assert.NotNull(firstLink);
            Guid g;
            Assert.True(Guid.TryParse(firstLink.Href.ToString(), out g));
            var firstPage = pages.SingleOrDefault(f => f.Id == (UuidIri)g);
            Assert.NotNull(firstPage);
            return firstPage;
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
