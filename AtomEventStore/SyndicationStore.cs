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
                var feed = new SyndicationFeed();
                this.headWriter.CreateOrUpdate(feed);

                var item = new SyndicationItem();
                item.Content = XmlSyndicationContent.CreateXmlContent(@event);
                this.entryWriter.Create(item);
            });
        }
    }
}
