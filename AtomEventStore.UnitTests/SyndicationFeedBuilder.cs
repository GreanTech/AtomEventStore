using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public class SyndicationFeedBuilder
    {
        private readonly string feedId;

        public SyndicationFeedBuilder()
            : this(Guid.NewGuid().ToString())
        {
        }

        private SyndicationFeedBuilder(string feedId)
        {
            this.feedId = feedId;
        }

        public SyndicationFeedBuilder WithFeedId(string feedId)
        {
            return new SyndicationFeedBuilder(feedId);
        }

        public SyndicationFeed Build()
        {
            return new SyndicationFeed { Id = this.feedId };
        }
    }
}
