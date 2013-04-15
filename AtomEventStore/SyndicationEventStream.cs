using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class SyndicationEventStream<T> : IEnumerable<T>
    {
        private readonly string id;
        private readonly ISyndicationFeedReader headReader;
        private readonly ISyndicationItemReader entryReader;
        private readonly ISyndicationItemWriter entryWriter;
        private readonly ISyndicationFeedWriter headWriter;

        public SyndicationEventStream(
            string id,
            ISyndicationFeedReader headReader,
            ISyndicationItemReader entryReader,
            ISyndicationItemWriter entryWriter,
            ISyndicationFeedWriter headWriter)
        {
            this.id = id;
            this.headReader = headReader;
            this.entryReader = entryReader;
            this.entryWriter = entryWriter;
            this.headWriter = headWriter;
        }

        public Task Append(T @event)
        {
            return Task.Factory.StartNew(() =>
            {
                var changesetId = UuidIri.NewId();
                var changesetAddress =
                    new Uri(((Guid)changesetId).ToString(), UriKind.Relative);

                var item = CreateItem(@event, changesetId, changesetAddress);
                this.AddPreviousLinkTo(item);

                this.entryWriter.Create(item);
                this.CreateOrUpdateHead(changesetAddress, item);
            });
        }

        private static SyndicationItem CreateItem(
            T @event,
            UuidIri changesetId,
            Uri changesetAddress)
        {
            var item = new SyndicationItem();
            item.Id = changesetId.ToString();
            item.Title = new TextSyndicationContent(
                "Changeset " + (Guid)changesetId);
            item.Links.Add(
                new SyndicationLink
                {
                    RelationshipType = "self",
                    Uri = changesetAddress
                });
            item.PublishDate = DateTimeOffset.Now;
            item.LastUpdatedTime = item.PublishDate;
            item.Authors.Add(new SyndicationPerson { Name = "Grean" });
            item.Content = XmlSyndicationContent.CreateXmlContent(@event);
            return item;
        }

        private void AddPreviousLinkTo(SyndicationItem item)
        {
            var head = this.headReader.ReadFeed(this.id);
            var headEntry = head.Items.FirstOrDefault();
            if (headEntry != null)
            {
                var headEntryLink =
                    headEntry.Links.Single(l => l.RelationshipType == "via");
                item.Links.Add(
                    new SyndicationLink
                    {
                        RelationshipType = "previous",
                        Uri = headEntryLink.Uri
                    });
            }
        }

        private void CreateOrUpdateHead(
            Uri changesetAddress,
            SyndicationItem item)
        {
            var feedItem = item.Clone();
            foreach (var l in feedItem.Links.Where(l => l.RelationshipType == "self"))
                l.RelationshipType = "via";

            var feed = new SyndicationFeed(new[] { feedItem });
            feed.Id = this.id;
            feed.Title = new TextSyndicationContent(
                "Head of event stream " + this.id);
            feed.Links.Add(
                new SyndicationLink
                {
                    RelationshipType = "self",
                    Uri = new Uri(this.id, UriKind.Relative)
                });
            feed.LastUpdatedTime = DateTimeOffset.Now;
            feed.Authors.Add(new SyndicationPerson { Name = "Grean" });
            this.headWriter.CreateOrUpdate(feed);
        }

        public string Id
        {
            get { return this.id; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var item = this.headReader.ReadFeed(this.id).Items.SingleOrDefault();
            while (item != null)
            {
                var content = (XmlSyndicationContent)item.Content;
                yield return content.ReadContent<T>();
                item = this.GetPrevious(item);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private SyndicationItem GetPrevious(SyndicationItem current)
        {
            var previousLink = current.Links.SingleOrDefault(
                l => l.RelationshipType == "previous");
            if (previousLink == null)
                return null;

            var previousId = previousLink.Uri.ToString();
            return this.entryReader.ReadItem(previousId);
        }
    }
}
