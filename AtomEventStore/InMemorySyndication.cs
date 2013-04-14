using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore
{
    public class InMemorySyndication : 
        ISyndicationFeedReader,
        ISyndicationFeedWriter
    {
        private SyndicationFeed feed;

        public SyndicationFeed Read(string id)
        {
            return this.feed ?? new SyndicationFeed();
        }

        public void CreateOrUpdate(SyndicationFeed feed)
        {
            this.feed = feed;
        }
    }
}
