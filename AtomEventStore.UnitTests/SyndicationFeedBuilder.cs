using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public class SyndicationFeedBuilder
    {
        public SyndicationFeed Build()
        {
            return new SyndicationFeed();
        }
    }
}
