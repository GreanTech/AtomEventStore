using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public static class AtomEnvy
    {
        public static Uri Locate(this AtomEntry atomEntry)
        {
            return atomEntry.Links.Single(l => l.IsSelfLink).Href;
        }

        public static Uri Locate(this AtomFeed atomFeed)
        {
            return atomFeed.Links.Single(l => l.IsSelfLink).Href;
        }
    }
}
