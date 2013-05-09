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
        private readonly Dictionary<Uri, StringBuilder> entries;

        public AtomEventsInMemory()
        {
            this.feeds = new Dictionary<Uri, StringBuilder>();
            this.entries = new Dictionary<Uri, StringBuilder>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AtomEntry", Justification = "This is a bug in the Code Analysis rule: http://bit.ly/17M7Jom")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "XmlWriter", Justification = "This is a bug in the Code Analysis rule: http://bit.ly/17M7Jom")]
        public XmlWriter CreateEntryWriterFor(AtomEntry atomEntry)
        {
            if (atomEntry == null)
                throw new ArgumentNullException("atomEntry");

            var href = GetHrefFrom(atomEntry.Links);
            if (this.entries.ContainsKey(href))
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Will not create a new XmlWriter for the supplied AtomEntry, because a an AtomEntry with the ID {0} was already written.",
                        href.ToString()));

            var sb = new StringBuilder();
            this.entries.Add(href, sb);
            return XmlWriter.Create(sb);
        }

        public XmlReader CreateEntryReaderFor(Uri href)
        {
            return CreateReaderOver(this.entries[href].ToString());
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
                UuidIri id = new Guid(href.ToString());
                return CreateReaderOver(
                    new AtomFeed(
                        id,
                        "Index of event stream " + (Guid)id,
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
        }

        private static Uri GetHrefFrom(IEnumerable<AtomLink> links)
        {
            var selfLink = links.Single(l => l.IsSelfLink);
            return selfLink.Href;
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

        public IEnumerable<string> Entries
        {
            get
            {
                return this.entries.Values.Select(sb => sb.ToString());
            }
        }
    }
}
