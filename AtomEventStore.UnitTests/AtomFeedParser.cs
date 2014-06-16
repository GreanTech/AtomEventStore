using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomFeedParser
    {
        public AtomFeed Parse(string xml)
        {
            return AtomFeed.Parse(
                xml,
                new ConventionBasedSerializerOfComplexImmutableClasses());
        }
    }
}
