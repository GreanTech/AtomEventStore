using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomFeedLikeness
    {
        private readonly DateTimeOffset minimumTime;
        private readonly UuidIri expectedId;
        private readonly object[] expectedEvents;

        public AtomFeedLikeness(
            DateTimeOffset minimumTime,
            UuidIri expectedId,
            params object[] expectedEvents)
        {
            this.minimumTime = minimumTime;
            this.expectedId = expectedId;
            this.expectedEvents = expectedEvents;
        }

        public override bool Equals(object obj)
        {
            var actual = obj as AtomFeed;
            if (actual == null)
                return base.Equals(obj);

            var expectedEntries = this.expectedEvents
                .Select(e => new AtomEntryLikeness(this.minimumTime, e))
                .Cast<object>();

            return object.Equals(this.expectedId, actual.Id)
                && (object.Equals("Index of event stream " + (Guid)this.expectedId, actual.Title) || "Partial event stream" == actual.Title)
                && this.minimumTime <= actual.Updated
                && actual.Updated <= DateTimeOffset.Now
                && expectedEntries.SequenceEqual(actual.Entries)
                && actual.Links.Contains(
                    AtomEventStream.CreateSelfLinkFrom(this.expectedId));
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
