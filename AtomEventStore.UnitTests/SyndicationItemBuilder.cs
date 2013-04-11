using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml.Serialization;

namespace Grean.AtomEventStore.UnitTests
{
    public class SyndicationItemBuilder
    {
        private readonly DateTimeOffset publishDate;
        private readonly SyndicationContent content;
        private readonly IEnumerable<SyndicationLink> links;

        public SyndicationItemBuilder()
            : this(
                DateTimeOffset.Now,
                SyndicationContent.CreatePlaintextContent(""),
                new []
                {
                    new SyndicationLink
                    { 
                        RelationshipType = "self",
                        Uri = new Uri(Guid.NewGuid().ToString(), UriKind.Relative)
                    }
                })
        {
        }

        private SyndicationItemBuilder(
            DateTimeOffset publishDate,
            SyndicationContent content,
            IEnumerable<SyndicationLink> links)
        {
            this.publishDate = publishDate;
            this.content = content;
            this.links = links;
        }

        public SyndicationItemBuilder WithXmlContent(object content)
        {
            var sc = XmlSyndicationContent.CreateXmlContent(content);
            return new SyndicationItemBuilder(this.publishDate, sc, this.links);
        }

        public SyndicationItem Build()
        {
            var item = new SyndicationItem();
            item.PublishDate = this.publishDate;
            item.LastUpdatedTime = this.publishDate;
            item.Content = this.content;
            foreach (var l in this.links)
                item.Links.Add(l);
            return item;
        }
    }
}
