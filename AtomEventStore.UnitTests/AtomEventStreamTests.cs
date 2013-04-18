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

                Assert.Equal(this.expectedId, actual.Id);
                Assert.Equal("Head of event stream " + (Guid)this.expectedId, actual.Title);
                Assert.True(this.minimumTime <= actual.Updated, "Updated should be very recent.");
                Assert.True(actual.Updated <= DateTimeOffset.Now, "Updated should not be in the future.");
                Assert.True(
                    expectedEntry.Equals(actual.Entries.Single()),
                    "Expected entry must match actual entry.");
                Assert.Contains(
                    AtomEventStream.CreateSelfLinkFrom(this.expectedId),
                    actual.Links);

                return true;
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

                Assert.NotEqual(default(UuidIri), actual.Id);
                Assert.Equal("Changeset " + (Guid)actual.Id, actual.Title);
                Assert.True(this.minimumTime <= actual.Published, "Published should be very recent.");
                Assert.True(actual.Published <= DateTimeOffset.Now, "Published should not be in the future.");
                Assert.True(this.minimumTime <= actual.Updated, "Updated should be very recent.");
                Assert.True(actual.Updated <= DateTimeOffset.Now, "Updated should not be in the future.");
                Assert.Contains(
                    AtomEventStream
                        .CreateSelfLinkFrom(actual.Id)
                        .WithRel(this.idRel),
                    actual.Links);
                Assert.Equal(this.expectedEvent, actual.Content.Item);

                return true;
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }
    }
}
