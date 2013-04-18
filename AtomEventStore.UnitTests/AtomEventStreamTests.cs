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
            AssertFeed(before, sut.Id, expectedEvent, writtenFeed);

            var writtenEntry = storage.Entries.Select(AtomEntry.Parse).Single();
            var expectedEntry = new AtomEntryLikeness(
                before,
                expectedEvent,
                new[]
                {
                    AtomEventStream.CreateSelfLinkFrom(writtenEntry.Id)
                });
            Assert.True(
                expectedEntry.Equals(writtenEntry),
                "Expected entry must match actual entry.");

            // Teardown
        }

        private static void AssertFeed(
            DateTimeOffset before,
            UuidIri expectedId,
            object expectedEvent,
            AtomFeed actual)
        {
            var expectedEntry = new AtomEntryLikeness(
                before,
                expectedEvent, 
                new[]
                {
                    AtomEventStream
                        .CreateSelfLinkFrom(
                            actual.Entries.Single().Id)
                        .ToViaLink()
                });

            Assert.Equal(expectedId, actual.Id);
            Assert.Equal("Head of event stream " + (Guid)expectedId, actual.Title);
            Assert.True(before <= actual.Updated, "Updated should be very recent.");
            Assert.True(actual.Updated <= DateTimeOffset.Now, "Updated should not be in the future.");
            Assert.True(
                expectedEntry.Equals(actual.Entries.Single()),
                "Expected entry must match actual entry.");
            Assert.Contains(
                AtomEventStream.CreateSelfLinkFrom(expectedId),
                actual.Links);
        }

        private class AtomEntryLikeness
        {
            private readonly DateTimeOffset minimumTime;
            private readonly object expectedEvent;
            private readonly ISet<AtomLink> expectedLinks;

            public AtomEntryLikeness(
                DateTimeOffset minimumTime,
                object expectedEvent,
                IEnumerable<AtomLink> expectedLinks)
            {
                this.minimumTime = minimumTime;
                this.expectedEvent = expectedEvent;
                this.expectedLinks = new HashSet<AtomLink>(expectedLinks);
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
                Assert.True(
                    this.expectedLinks.IsSubsetOf(actual.Links),
                    "Expected links must be contained in actual links.");
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
