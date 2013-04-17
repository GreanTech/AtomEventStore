using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEntryComparer : IEqualityComparer<AtomEntry>
    {
        public bool Equals(AtomEntry x, AtomEntry y)
        {
            return object.Equals(x.Id, y.Id)
                && object.Equals(x.Title, y.Title)
                && object.Equals(x.Published, y.Published)
                && object.Equals(x.Updated, y.Updated)
                && object.Equals(x.Author, y.Author)
                && object.Equals(x.Content, y.Content)
                && new HashSet<AtomLink>(x.Links).SetEquals(y.Links);
        }

        public int GetHashCode(AtomEntry obj)
        {
            return 0;
        }
    }
}
