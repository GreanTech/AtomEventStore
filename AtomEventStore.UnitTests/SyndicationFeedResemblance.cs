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
                return true;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
