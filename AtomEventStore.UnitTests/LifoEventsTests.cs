using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using Ploeh.AutoFixture;

namespace Grean.AtomEventStore.UnitTests
{
    public class LifoEventsTests
    {
        [Theory, AutoAtomData]
        public void SutIsEnumerable(LifoEvents<XmlAttributedTestEventX> sut)
        {
            Assert.IsAssignableFrom<IEnumerable<XmlAttributedTestEventX>>(sut);
        }

        [Theory, AutoAtomData]
        public void PropertiesAreCorrectlyInitialized(
            ConstructorInitializedMemberAssertion assertion)
        {
            assertion.Verify(typeof(LifoEvents<XmlAttributedTestEventX>));
        }

        [Theory, AutoAtomData]
        public void SutIsInitiallyEmpty(
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            LifoEvents<XmlAttributedTestEventX> sut)
        {
            Assert.False(sut.Any(), "Intial event stream should be empty.");
            Assert.Empty(sut);
        }

        [Theory, AutoAtomData]
        public void SutYieldsCorrectEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            [Frozen]UuidIri dummyId,
            AtomEventObserver<XmlAttributedTestEventX> writer,
            LifoEvents<XmlAttributedTestEventX> sut,
            List<XmlAttributedTestEventX> expected)
        {
            Enumerable
                .Reverse(expected)
                .ToList()
                .ForEach(e => writer.AppendAsync(e).Wait());

            Assert.True(
                expected.SequenceEqual(sut),
                "Events should be yielded in a LIFO order");
            Assert.True(
                expected.Cast<object>().SequenceEqual(sut.OfType<object>()),
                "Events should be yielded in a LIFO order");
        }

        [Theory, AutoAtomData]
        public void SutYieldsPagedEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            [Frozen]UuidIri dummyId,
            AtomEventObserver<XmlAttributedTestEventX> writer,
            LifoEvents<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var expected = eventGenerator.Take(writer.PageSize * 2 + 1).ToList();
            Enumerable
                .Reverse(expected)
                .ToList()
                .ForEach(e => writer.AppendAsync(e).Wait());

            Assert.True(
                expected.SequenceEqual(sut),
                "Events should be yielded in a LIFO order");
            Assert.True(
                expected.Cast<object>().SequenceEqual(sut.OfType<object>()),
                "Events should be yielded in a LIFO order");
        }

        [Theory, AutoAtomData]
        public void SutCanAppendAndYieldPolymorphicEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            [Frozen]UuidIri dummyId,
            AtomEventObserver<IXmlAttributedTestEvent> writer,
            LifoEvents<IXmlAttributedTestEvent> sut,
            XmlAttributedTestEventX tex,
            XmlAttributedTestEventY tey)
        {
            writer.AppendAsync(tex).Wait();
            writer.AppendAsync(tey).Wait();

            var expected = new IXmlAttributedTestEvent[] { tey, tex };
            Assert.True(expected.SequenceEqual(sut));
        }

        [Theory, AutoAtomData]
        public void ReverseYieldsCorrectEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            [Frozen]UuidIri dummyId,
            AtomEventObserver<XmlAttributedTestEventX> writer,
            LifoEvents<XmlAttributedTestEventX> sut,
            List<XmlAttributedTestEventX> expected)
        {
            expected.ForEach(e => writer.AppendAsync(e).Wait());

            var actual = sut.Reverse();

            Assert.True(
                expected.SequenceEqual(actual),
                "Events should be yielded in a FIFO order");
            Assert.True(
                expected.Cast<object>().SequenceEqual(actual.OfType<object>()),
                "Events should be yielded in a FIFO order");
        }

        [Theory, AutoAtomData]
        public void ReverseReturnsCorrectResult(
            UuidIri id,
            AtomEventsInMemory storage,
            XmlContentSerializer serializer)
        {
            var sut =
                new LifoEvents<XmlAttributedTestEventX>(id, storage, serializer);
            var expected =
                new FifoEvents<XmlAttributedTestEventX>(id, storage, serializer);

            var actual = sut.Reverse();

            var fifo = Assert.IsType<FifoEvents<XmlAttributedTestEventX>>(actual);
            Assert.Equal(expected.Id, fifo.Id);
            Assert.Equal(expected.Storage, fifo.Storage);
            Assert.Equal(expected.Serializer, fifo.Serializer);
        }

        [Theory]
        [InlineAutoAtomData(2, 1)]
        [InlineAutoAtomData(3, 1)]
        [InlineAutoAtomData(3, 2)]
        public void EnumerationStartsFromMostRecentEventEvenIfLastLinkIsStale(
            int pageCount,
            int staleCount,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            [Frozen]UuidIri id,
            LifoEvents<IXmlAttributedTestEvent> sut,
            AtomEventObserver<XmlAttributedTestEventX> writer,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            // Fixture setup
            var events = 
                eventGenerator.Take(writer.PageSize * pageCount + 1).ToList();
            events.ForEach(writer.OnNext);

            /* Point the 'last' link to an older page, instead of to the last
             * page. This simulates that when the true last page was created,
             * the index wasn't correctly updated. This could for example
             * happen due to a network failure. */
            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var lastPage = FindLastPage(writtenFeeds, id);
            var olderPage = 
                FindPreviousPage(lastPage, writtenFeeds, staleCount);
            var staleLastLink =
                olderPage.Links.Single(l => l.IsSelfLink).ToLastLink();
            var index = FindIndex(writtenFeeds, id);
            index = index.WithLinks(index.Links
                .Where(l => !l.IsLastLink)
                .Concat(new[] { staleLastLink }));
            using (var w = storage.CreateFeedWriterFor(index))
                index.WriteTo(w, sut.Serializer);

            // Exercise system
            /* (The method being exercised is actual GetEnumerator and the
             * returned implementation of IEnumerator<T>, but ToList or ToArray
             * or similar methods triggers that.) */
            var actual = sut.ToList();

            // Verify outcome
            var expected = events.AsEnumerable().Reverse();
            Assert.True(
                expected.SequenceEqual(actual),
                "All written events should be enumerated in correct order.");
        }

        private static AtomFeed ParseAtomFeed(string xml)
        {
            return AtomFeed.Parse(
                xml,
                new XmlContentSerializer(new TestEventTypeResolver()));
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

        private static AtomFeed FindPreviousPage(AtomFeed page, IEnumerable<AtomFeed> pages)
        {
            var previousLink = page.Links.SingleOrDefault(l => l.IsPreviousLink);
            Assert.NotNull(previousLink);
            var previousPage = pages.SingleOrDefault(f => f.Links.Single(l => l.IsSelfLink).Href == previousLink.Href);
            Assert.NotNull(previousPage);
            return previousPage;
        }

        private static AtomFeed FindPreviousPage(AtomFeed page, IEnumerable<AtomFeed> pages, int count)
        {
            var p = page;
            for (int i = 0; i < count; i++)
                p = FindPreviousPage(p, pages);

            return p;
        }

        private static AtomFeed FindLastPage(IEnumerable<AtomFeed> pages, UuidIri id)
        {
            var page = FindFirstPage(pages, id);
            while (page.Links.Any(l => l.IsNextLink))
                page = FindNextPage(page, pages);

            return page;
        }
    }
}
