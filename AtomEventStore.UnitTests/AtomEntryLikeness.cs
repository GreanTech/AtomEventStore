using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEntryLikeness
    {
        private readonly DateTimeOffset minimumTime;
        private readonly object expectedEvent;

        public AtomEntryLikeness(
            DateTimeOffset minimumTime,
            object expectedEvent)
        {
            this.minimumTime = minimumTime;
            this.expectedEvent = expectedEvent;
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
                && object.Equals(this.expectedEvent, actual.Content.Item);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
