using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public static class SyndicationEnvy
    {
        public static SyndicationItemResemblance ToResemblance(
            this SyndicationItem syndicationItem)
        {
            return new SyndicationItemResemblance(syndicationItem);
        }

        public static SyndicationFeedResemblance ToResemblance(
            this SyndicationFeed syndicationFeed)
        {
            return new SyndicationFeedResemblance(syndicationFeed);
        }
    }
}
