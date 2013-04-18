using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class AtomEventsInMemory : IAtomEventPersistence
    {
        private readonly Dictionary<UuidIri, StringBuilder> feeds;
        private readonly Dictionary<UuidIri, StringBuilder> entries;

        public AtomEventsInMemory()
        {
            this.feeds = new Dictionary<UuidIri, StringBuilder>();
            this.entries = new Dictionary<UuidIri, StringBuilder>();
        }

        public XmlWriter CreateEntryWriterFor(AtomEntry atomEntry)
        {
            var id = GetIdFrom(atomEntry.Links);
            if (this.entries.ContainsKey(id))
                throw new InvalidOperationException(
                    string.Format(
                        "Will not create a new XmlWriter for the supplied AtomEntry, because a an AtomEntry with the ID {0} was already written.",
                        id.ToString()));

            var sb = new StringBuilder();
            this.entries.Add(id, sb);
            return XmlWriter.Create(sb);
        }

        public XmlReader CreateEntryReaderFor(UuidIri id)
        {
            return CreateReaderOver(this.entries[id].ToString());
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            var id = GetIdFrom(atomFeed.Links);
            var sb = new StringBuilder();
            this.feeds[id] = sb;
            return XmlWriter.Create(sb);
        }

        public XmlReader CreateFeedReaderFor(UuidIri id)
        {
            if (this.feeds.ContainsKey(id))
                return CreateReaderOver(this.feeds[id].ToString());
            else
                return CreateReaderOver(
                    new AtomFeed(
                        id,
                        "Head of event stream " + (Guid)id,
                        DateTimeOffset.Now,
                        new AtomAuthor("Grean"),
                        Enumerable.Empty<AtomEntry>(),
                        new[]
                        {
                            AtomLink.CreateSelfLink(
                                new Uri(
                                    ((Guid)id).ToString(),
                                    UriKind.Relative))
                        })
                    .ToXmlString());
        }

        private static UuidIri GetIdFrom(IEnumerable<AtomLink> links)
        {
            var selfLink = links.Single(l => l.IsSelfLink);
            return new Guid(selfLink.Href.ToString());
        }

        private static XmlReader CreateReaderOver(string xml)
        {
            var sr = new StringReader(xml);
            return XmlReader.Create(
                sr,
                new XmlReaderSettings { CloseInput = true });
        }
    }
}
