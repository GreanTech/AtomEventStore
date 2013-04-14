using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore
{
    public class InMemorySyndication : 
        ISyndicationFeedReader,
        ISyndicationFeedWriter,
        ISyndicationItemWriter
    {
        private readonly Dictionary<string, SyndicationFeed> feeds;

        public InMemorySyndication()
        {
            this.feeds = new Dictionary<string, SyndicationFeed>();
        }

        public SyndicationFeed Read(string id)
        {
            SyndicationFeed feed;
            if (this.feeds.TryGetValue(id, out feed))
                return feed;

            return new SyndicationFeed();
        }

        public void CreateOrUpdate(SyndicationFeed feed)
        {
            var feedId = GetId(feed);
            this.feeds[feedId] = feed;
        }

        private static string GetId(SyndicationFeed feed)
        {
            var selfLink = feed.Links.Single(l => l.RelationshipType == "self");
            return selfLink.Uri.ToString();
        }

        public void Create(SyndicationItem item)
        {
        }
    }
}
