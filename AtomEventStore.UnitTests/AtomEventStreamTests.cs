using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventStreamTests
    {
        [Theory, AutoAtomData]
        public void IdIsCorrect(
            [Frozen]UuidIri expected,
            AtomEventStream<TestEventX> sut)
        {
            UuidIri actual = sut.Id;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void StorageIsCorrect(
            [Frozen]IAtomEventStorage expected,
            AtomEventStream<TestEventX> sut)
        {
            IAtomEventStorage actual = sut.Storage;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void CreateSelfLinkFromReturnsCorrectResult(
            UuidIri id)
        {
            AtomLink actual = AtomEventStream.CreateSelfLinkFrom(id);

            var expected = AtomLink.CreateSelfLink(
                new Uri(
                    ((Guid)id).ToString(),
                    UriKind.Relative));
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void AppendAsyncCorrectlyStoresFeedAndEntry(
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventStream<TestEventX> sut,
            TestEventX expectedEvent)
        {
            // Fixture setup
            var before = DateTimeOffset.Now;

            // Exercise system
            sut.AppendAsync(expectedEvent).Wait();

            // Verify outcome
            var writtenFeed = storage.Feeds.Select(AtomFeed.Parse).Single();
            var expectedFeed = new AtomFeedLikeness(before, sut.Id, expectedEvent);
            Assert.True(
                expectedFeed.Equals(writtenFeed),
                "Expected feed must match actual feed.");

            var writtenEntry = storage.Entries.Select(AtomEntry.Parse).Single();
            var expectedEntry = new AtomEntryLikeness(
                before,
                expectedEvent,
                "self");
            Assert.True(
                expectedEntry.Equals(writtenEntry),
                "Expected entry must match actual entry.");

            // Teardown
        }

        [Theory, AutoAtomData]
        public void AppendAsyncCorrectlyStoresFeedAndEntries(
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventStream<TestEventX> sut,
            TestEventX event1,
            TestEventX event2)
        {
            // Fixture setup
            var before = DateTimeOffset.Now;

            // Exercise system
            sut.AppendAsync(event1).Wait();
            sut.AppendAsync(event2).Wait();

            // Verify outcome
            var writtenFeed = storage.Feeds.Select(AtomFeed.Parse).Single();
            var writtenEntries = storage.Entries.Select(AtomEntry.Parse);

            var expectedFeed = new AtomFeedLikeness(before, sut.Id, event2);
            var expectedEntries = new HashSet<object>(
                new[]
                {
                    new AtomEntryLikeness(before, event1, "self"),
                    new AtomEntryLikeness(before, event2, "self")
                },
                new HashFreeEqualityComparer<object>());

            Assert.True(expectedFeed.Equals(writtenFeed));
            Assert.True(expectedEntries.SetEquals(writtenEntries));
            // Teardown
        }

        [Theory, AutoAtomData]
        public void AppendAsyncCorrectlyLinksSecondChangesetToFirst(
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventStream<TestEventX> sut,
            TestEventX event1,
            TestEventX event2)
        {
            // Fixture setup

            // Exercise system
            sut.AppendAsync(event1).Wait();
            sut.AppendAsync(event2).Wait();

            // Verify outcome
            var writtenFeed = storage.Feeds.Select(AtomFeed.Parse).Single();
            var writtenEntries = storage.Entries.Select(AtomEntry.Parse);

            var headLink = writtenFeed
                .Entries.First()
                .Links.FirstOrDefault(l => l.IsViaLink);
            Assert.NotNull(headLink);
            var head = writtenEntries.Single(
                e => e.Links.Any(l => headLink.ToSelfLink().Equals(l)));
            var previousLink = head.Links.SingleOrDefault(l => l.Rel == "previous");
            Assert.NotNull(previousLink);
            var previous = writtenEntries.Single(
                e => e.Links.Any(l => previousLink.ToSelfLink().Equals(l)));
            Assert.False(
                previous.Links.Any(l => l.Rel == "previous"),
                "First entry can't have a previous link.");
            // Teardown
        }

        private class AtomFeedLikeness
        {
            private readonly DateTimeOffset minimumTime;
            private readonly UuidIri expectedId;
            private readonly object expectedEvent;

            public AtomFeedLikeness(
                DateTimeOffset minimumTime,
                UuidIri expectedId,
                object expectedEvent)
            {
                this.minimumTime = minimumTime;
                this.expectedId = expectedId;
                this.expectedEvent = expectedEvent;
            }

            public override bool Equals(object obj)
            {
                var actual = obj as AtomFeed;
                if (actual == null)
                    return base.Equals(obj);

                var expectedEntry = new AtomEntryLikeness(
                    this.minimumTime,
                    this.expectedEvent,
                    "via");

                return object.Equals(this.expectedId, actual.Id)
                    && object.Equals("Head of event stream " + (Guid)this.expectedId, actual.Title)
                    && this.minimumTime <= actual.Updated
                    && actual.Updated <= DateTimeOffset.Now
                    && object.Equals(expectedEntry, actual.Entries.Single())
                    && actual.Links.Contains(
                        AtomEventStream.CreateSelfLinkFrom(this.expectedId));
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }

        private class AtomEntryLikeness
        {
            private readonly DateTimeOffset minimumTime;
            private readonly object expectedEvent;
            private readonly string idRel;

            public AtomEntryLikeness(
                DateTimeOffset minimumTime,
                object expectedEvent,
                string idRel)
            {
                this.minimumTime = minimumTime;
                this.expectedEvent = expectedEvent;
                this.idRel = idRel;
            }

            public override bool Equals(object obj)
            {
                var actual = obj as AtomEntry;
                if (actual == null)
                    return base.Equals(obj);

                return !object.Equals(default(UuidIri), actual.Id)
                    && object.Equals("Changeset " + (Guid)actual.Id, actual.Title)
                    && this.minimumTime <= actual.Published
                    && actual.Published <= DateTimeOffset.Now
                    && this.minimumTime <= actual.Updated
                    && actual.Updated <= DateTimeOffset.Now
                    && actual.Links.Contains(
                        AtomEventStream
                            .CreateSelfLinkFrom(actual.Id)
                            .WithRel(this.idRel))
                    && object.Equals(this.expectedEvent, actual.Content.Item);
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }

        private class HashFreeEqualityComparer<T> : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                return object.Equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return 0;
            }
        }

    }
}
