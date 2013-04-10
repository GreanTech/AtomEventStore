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

        public SyndicationItemBuilder()
            : this(
                DateTimeOffset.Now,
                SyndicationContent.CreatePlaintextContent(""))
        {
        }

        private SyndicationItemBuilder(
            DateTimeOffset publishDate,
            SyndicationContent content)
        {
            this.publishDate = publishDate;
            this.content = content;
        }

        public SyndicationItemBuilder WithXmlContent(object content)
        {
            var sc = XmlSyndicationContent.CreateXmlContent(content);
            return new SyndicationItemBuilder(this.publishDate, sc);
        }

        public SyndicationItem Build()
        {
            return new SyndicationItem
            {
                PublishDate = this.publishDate,
                LastUpdatedTime = this.publishDate,
                Content = this.content 
            };
        }
    }
}
