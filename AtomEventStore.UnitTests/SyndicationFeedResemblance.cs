using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Grean.AtomEventStore.UnitTests
{
    public class SyndicationFeedResemblance : SyndicationFeed
    {
        private readonly SyndicationFeed feed;

        public SyndicationFeedResemblance(SyndicationFeed feed)
        {
            this.feed = feed;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SyndicationFeed;
            if (other != null)
                return HasCorrectAuthors(other.Authors);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        private static bool HasCorrectAuthors(
            IEnumerable<SyndicationPerson> candidate)
        {
            return candidate.Any(p => !string.IsNullOrWhiteSpace(p.Name));
        }
    }
}
