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
            var feed = new SyndicationFeed();
            feed.Id = this.feedId;
            feed.Links.Add(
                new SyndicationLink
                {
                    RelationshipType = "self",
                    Uri = new Uri(this.feedId, UriKind.Relative)
                });
            feed.LastUpdatedTime = DateTimeOffset.Now;

            return feed;
        }
    }
}
