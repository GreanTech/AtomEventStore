using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore
{
    public class InMemorySyndication : ISyndicationFeedReader
    {
        public SyndicationFeed Read(string id)
        {
            throw new NotImplementedException();
        }
    }
}
