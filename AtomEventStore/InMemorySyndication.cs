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
        ISyndicationItemWriter,
        ISyndicationItemReader
    {
        private readonly Dictionary<string, SyndicationFeed> feeds;
        private readonly Dictionary<string, SyndicationItem> items;

        public InMemorySyndication()
        {
            this.feeds = new Dictionary<string, SyndicationFeed>();
            this.items = new Dictionary<string, SyndicationItem>();
        }

        public SyndicationFeed ReadFeed(string id)
        {
            SyndicationFeed feed;
            if (this.feeds.TryGetValue(id, out feed))
                return feed;

            return new SyndicationFeed();
        }

        public void CreateOrUpdate(SyndicationFeed feed)
        {
            var feedId = GetId(feed.Links);
            this.feeds[feedId] = feed;
        }

        private static string GetId(IEnumerable<SyndicationLink> links)
        {
            var selfLink = links.Single(l => l.RelationshipType == "self");
            return selfLink.Uri.ToString();
        }

        public void Create(SyndicationItem item)
        {
            var itemId = GetId(item.Links);
            this.items.Add(itemId, item);
        }

        public SyndicationItem ReadItem(string id)
        {
            return this.items[id];
        }
    }
}
