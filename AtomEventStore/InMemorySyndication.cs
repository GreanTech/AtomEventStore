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
        private readonly List<string> itemIds;

        public InMemorySyndication()
        {
            this.feeds = new Dictionary<string, SyndicationFeed>();
            this.itemIds = new List<string>();
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
            if (this.itemIds.Contains(itemId))
                throw new ArgumentException(
                    string.Format(
                        "A SyndicationItem with the ID \"{0}\" was already created.",
                        itemId),
                    "item");

            this.itemIds.Add(itemId);
        }
    }
}
