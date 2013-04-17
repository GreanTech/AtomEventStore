using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomFeedComparer : IEqualityComparer<AtomFeed>
    {
        private readonly AtomEntryComparer entryComparer;

        public AtomFeedComparer()
        {
            this.entryComparer = new AtomEntryComparer();
        }

        public bool Equals(AtomFeed x, AtomFeed y)
        {
            return object.Equals(x.Id, y.Id)
                && object.Equals(x.Title, x.Title)
                && object.Equals(x.Updated, y.Updated)
                && object.Equals(x.Author, y.Author)
                && x.Entries.SequenceEqual(y.Entries, this.entryComparer)
                && new HashSet<AtomLink>(x.Links).SetEquals(y.Links);
        }

        public int GetHashCode(AtomFeed obj)
        {
            return 0;
        }
    }
}
