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
        private readonly SyndicationContent content;

        public SyndicationItemBuilder()
        {
            this.content = SyndicationContent.CreatePlaintextContent("");
        }

        private SyndicationItemBuilder(SyndicationContent content)
        {
            this.content = content;
        }

        public SyndicationItemBuilder WithXmlContent(object content)
        {
            var sc = XmlSyndicationContent.CreateXmlContent(content);
            return new SyndicationItemBuilder(sc);
        }

        public SyndicationItem Build()
        {
            return new SyndicationItem { Content = this.content };
        }
    }
}
