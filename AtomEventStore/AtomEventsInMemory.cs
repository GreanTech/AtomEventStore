using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class AtomEventsInMemory : IAtomEventStorage
    {
        private readonly Dictionary<Uri, StringBuilder> feeds;

        public AtomEventsInMemory()
        {
            this.feeds = new Dictionary<Uri, StringBuilder>();
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            if (atomFeed == null)
                throw new ArgumentNullException("atomFeed");

            var id = GetHrefFrom(atomFeed.Links);
            var sb = new StringBuilder();
            this.feeds[id] = sb;
            return XmlWriter.Create(sb);
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            if (href == null)
                throw new ArgumentNullException("href");

            if (this.feeds.ContainsKey(href))
                return CreateReaderOver(this.feeds[href].ToString());
            else
            {
                UuidIri id = GetIdFromHref(href);
                var xml = new AtomFeed(
                    id,
                    "Index of event stream " + (Guid)id,
                    DateTimeOffset.Now,
                    new AtomAuthor("Grean"),
                    Enumerable.Empty<AtomEntry>(),
                    new[]
                    {
                        AtomLink.CreateSelfLink(href)
                    })
                .ToXmlString((IContentSerializer)null);
                return CreateReaderOver(xml);
            }
        }

        private static Uri GetHrefFrom(IEnumerable<AtomLink> links)
        {
            var selfLink = links.Single(l => l.IsSelfLink);
            return selfLink.Href;
        }

        private static Guid GetIdFromHref(Uri href)
        {
            return new Guid(href.ToString());
        }

        private static XmlReader CreateReaderOver(string xml)
        {
            var sr = new StringReader(xml);
            try
            {
                return XmlReader.Create(
                    sr,
                    new XmlReaderSettings { CloseInput = true });
            }
            catch
            {
                sr.Dispose();
                throw;
            }
        }

        public IEnumerable<string> Feeds
        {
            get
            {
                return this.feeds.Values.Select(sb => sb.ToString());
            }
        }
    }
}
