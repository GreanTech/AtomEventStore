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
        private readonly ISyndicationItemWriter entryWriter;
        private readonly ISyndicationFeedWriter headWriter;

        public SyndicationEventStream(
            string id,
            ISyndicationFeedReader headReader,
            ISyndicationItemWriter entryWriter,
            ISyndicationFeedWriter headWriter)
        {
            this.id = id;
            this.headReader = headReader;
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
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
