using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Grean.AtomEventStore
{
    public class AtomFeed : IXmlWritable
    {
        private readonly UuidIri id;
        private readonly string title;
        private readonly DateTimeOffset updated;
        private readonly AtomAuthor author;
        private readonly IEnumerable<AtomEntry> entries;
        private readonly IEnumerable<AtomLink> links;

        public AtomFeed(
            UuidIri id,
            string title, 
            DateTimeOffset updated,
            AtomAuthor author,
            IEnumerable<AtomEntry> entries,
            IEnumerable<AtomLink> links)
        {
            if (title == null)
                throw new ArgumentNullException("title");
            if (author == null)
                throw new ArgumentNullException("author");
            if (entries == null)
                throw new ArgumentNullException("entries");
            if (links == null)
                throw new ArgumentNullException("links");

            this.id = id;
            this.title = title;
            this.updated = updated;
            this.author = author;
            this.entries = entries;
            this.links = links;
        }

        public UuidIri Id
        {
            get { return this.id; }
        }

        public string Title
        {
            get { return this.title; }
        }

        public DateTimeOffset Updated
        {
            get { return this.updated; }
        }

        public AtomAuthor Author
        {
            get { return this.author; }
        }

        public IEnumerable<AtomEntry> Entries
        {
            get { return this.entries; }
        }

        public IEnumerable<AtomLink> Links
        {
            get { return this.links; }
        }

        public AtomFeed WithTitle(string newTitle)
        {
            return new AtomFeed(
                this.id,
                newTitle,
                this.updated,
                this.author,
                this.entries,
                this.links);
        }

        public AtomFeed WithUpdated(DateTimeOffset newUpdated)
        {
            return new AtomFeed(
                this.id,
                this.title,
                newUpdated,
                this.author,
                this.entries,
                this.links);
        }

        public AtomFeed WithAuthor(AtomAuthor newAuthor)
        {
            return new AtomFeed(
                this.id,
                this.title,
                this.updated,
                newAuthor,
                this.entries,
                this.links);
        }

        public AtomFeed WithEntries(IEnumerable<AtomEntry> newEntries)
        {
            return new AtomFeed(
                this.id,
                this.title,
                this.updated,
                this.author,
                newEntries,
                this.links);
        }

        public AtomFeed WithLinks(IEnumerable<AtomLink> newLinks)
        {
            return new AtomFeed(
                this.id,
                this.title,
                this.updated,
                this.author,
                this.entries,
                newLinks);
        }

        public void WriteTo(XmlWriter xmlWriter, IContentSerializer serializer)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");

            xmlWriter.WriteStartElement("feed", "http://www.w3.org/2005/Atom");

            xmlWriter.WriteElementString("id", this.id.ToString());

            xmlWriter.WriteStartElement("title");
            xmlWriter.WriteAttributeString("type", "text");
            xmlWriter.WriteString(this.title);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteElementString(
                "updated",
                this.updated.ToString(
                    "o",
                    CultureInfo.InvariantCulture));

            this.author.WriteTo(xmlWriter);

            this.WriteLinksTo(xmlWriter);

            this.WriteEntriesTo(xmlWriter, serializer);

            xmlWriter.WriteEndElement();
        }

        private void WriteLinksTo(XmlWriter xmlWriter)
        {
            foreach (var l in this.links)
                l.WriteTo(xmlWriter);
        }

        private void WriteEntriesTo(XmlWriter xmlWriter, IContentSerializer serializer)
        {
            foreach (var e in this.entries)
                e.WriteTo(xmlWriter, serializer);
        }

        public static AtomFeed ReadFrom(XmlReader xmlReader)
        {
            return ReadFrom(
                xmlReader,
                new ConventionBasedSerializerOfComplexImmutableClasses());
        }

        public static AtomFeed ReadFrom(
            XmlReader xmlReader,
            IContentSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var navigator = new XPathDocument(xmlReader).CreateNavigator();

            var resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            var id = navigator
                .Select("/atom:feed/atom:id", resolver).Cast<XPathNavigator>()
                .Single().Value;
            var title = navigator
                .Select("/atom:feed/atom:title[@type = 'text']", resolver).Cast<XPathNavigator>()
                .Single().Value;
            var updated = navigator
                .Select("/atom:feed/atom:updated", resolver).Cast<XPathNavigator>()
                .Single().Value;
            var author = navigator
                .Select("/atom:feed/atom:author", resolver).Cast<XPathNavigator>()
                .Single().ReadSubtree();
            var entries = navigator
                .Select("/atom:feed/atom:entry", resolver).Cast<XPathNavigator>();
            var links = navigator
                .Select("/atom:feed/atom:link", resolver).Cast<XPathNavigator>();

            return new AtomFeed(
                UuidIri.Parse(id),
                title,
                DateTimeOffset.Parse(updated, CultureInfo.InvariantCulture),
                AtomAuthor.ReadFrom(author),
                entries.Select(x => AtomEntry.ReadFrom(x.ReadSubtree(), serializer)),
                links.Select(x => AtomLink.ReadFrom(x.ReadSubtree())));
        }

        public AtomFeed AddLink(AtomLink newLink)
        {
            if (newLink == null)
                throw new ArgumentNullException("newLink");
            
            return this.WithLinks(this.links.Concat(new[] { newLink }));
        }

        public static AtomFeed Parse(string xml)
        {
            return Parse(
                xml,
                new ConventionBasedSerializerOfComplexImmutableClasses());
        }

        public static AtomFeed Parse(string xml, IContentSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var sr = new StringReader(xml);
            try
            {
                using (var r = XmlReader.Create(sr))
                {
                    sr = null;
                    return AtomFeed.ReadFrom(r);
                }
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
            }
        }
    }
}
