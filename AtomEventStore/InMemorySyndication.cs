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
        public SyndicationFeed Read(string id)
        {
            return new SyndicationFeed();
        }

        public void CreateOrUpdate(SyndicationFeed feed)
        {
            throw new NotImplementedException();
        }
    }
}
