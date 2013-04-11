using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class SyndicationStore
    {
        private readonly ISyndicationItemWriter entryWriter;
        private readonly ISyndicationFeedWriter headWriter;

        public SyndicationStore(
            ISyndicationItemWriter entryWriter,
            ISyndicationFeedWriter headWriter)
        {
            this.entryWriter = entryWriter;
            this.headWriter = headWriter;
        }

        public Task Append(string id, object @event)
        {
            return Task.Factory.StartNew(() =>
            {
                var changesetId = UuidIri.NewId();

                var item = new SyndicationItem();
                item.Id = changesetId.ToString();
                item.Title = new TextSyndicationContent(
                    "Changeset " + (Guid)changesetId);
                item.PublishDate = DateTimeOffset.Now;
                item.LastUpdatedTime = item.PublishDate;
                item.Authors.Add(new SyndicationPerson { Name = "Grean" });
                item.Content = XmlSyndicationContent.CreateXmlContent(@event);
                this.entryWriter.Create(item);

                var feed = new SyndicationFeed();
                feed.Authors.Add(new SyndicationPerson { Name = "Grean" });
                this.headWriter.CreateOrUpdate(feed);
            });
        }
    }
}
