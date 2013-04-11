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
        private readonly IEnumerable<SyndicationItem> items;

        public SyndicationFeedBuilder()
            : this(
                Guid.NewGuid().ToString(),
                Enumerable.Empty<SyndicationItem>())
        {
        }

        private SyndicationFeedBuilder(
            string feedId,
            IEnumerable<SyndicationItem> items)
        {
            this.feedId = feedId;
            this.items = items;
        }

        public SyndicationFeedBuilder WithFeedId(string newFeedId)
        {
            return new SyndicationFeedBuilder(newFeedId, this.items);
        }

        public SyndicationFeedBuilder WithItem(SyndicationItem newItem)
        {
            return new SyndicationFeedBuilder(
                this.feedId,
                this.items.Concat(new[] { newItem }));
        }

        public SyndicationFeed Build()
        {
            var feed = new SyndicationFeed(this.items.ToList());
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
