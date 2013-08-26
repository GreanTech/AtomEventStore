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
    public class AtomEntry : IXmlWritable
    {
        private readonly UuidIri id;
        private readonly string title;
        private readonly DateTimeOffset published;
        private readonly DateTimeOffset updated;
        private readonly AtomAuthor author;
        private readonly XmlAtomContent content;
        private readonly IEnumerable<AtomLink> links;

        public AtomEntry(
            UuidIri id,
            string title,
            DateTimeOffset published,
            DateTimeOffset updated,
            AtomAuthor author,
            XmlAtomContent content,
            IEnumerable<AtomLink> links)
        {
            if (title == null)
                throw new ArgumentNullException("title");
            if (author == null)
                throw new ArgumentNullException("author");
            if (content == null)
                throw new ArgumentNullException("content");
            if (links == null)
                throw new ArgumentNullException("links");
            
            this.id = id;
            this.title = title;
            this.published = published;
            this.updated = updated;
            this.author = author;
            this.content = content;
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

        public DateTimeOffset Published
        {
            get { return this.published; }
        }

        public DateTimeOffset Updated
        {
            get { return this.updated; }
        }

        public AtomAuthor Author
        {
            get { return this.author; }
        }

        public XmlAtomContent Content
        {
            get { return this.content; }
        }

        public IEnumerable<AtomLink> Links
        {
            get { return this.links; }
        }

        public AtomEntry WithTitle(string newTitle)
        {
            return new AtomEntry(
                this.id,
                newTitle,
                this.published,
                this.updated,
                this.author,
                this.content,
                this.links);
        }

        public AtomEntry WithUpdated(DateTimeOffset newUpdated)
        {
            return new AtomEntry(
                this.id,
                this.title,
                this.published,
                newUpdated,
                this.author,
                this.content,
                this.links);
        }

        public AtomEntry WithAuthor(AtomAuthor newAuthor)
        {
            return new AtomEntry(
                this.id,
                this.title,
                this.published,
                this.updated,
                newAuthor,
                this.content,
                this.links);
        }

        public AtomEntry WithContent(XmlAtomContent newContent)
        {
            return new AtomEntry(
                this.id,
                this.title,
                this.published,
                this.updated,
                this.author,
                newContent,
                this.links);
        }

        public AtomEntry WithLinks(IEnumerable<AtomLink> newLinks)
        {
            return new AtomEntry(
                this.id,
                this.title,
                this.published,
                this.updated,
                this.author,
                this.content,
                newLinks);
        }

        public void WriteTo(XmlWriter xmlWriter, IContentSerializer serializer)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            xmlWriter.WriteStartElement("entry", "http://www.w3.org/2005/Atom");

            xmlWriter.WriteElementString("id", this.id.ToString());

            xmlWriter.WriteStartElement("title");
            xmlWriter.WriteAttributeString("type", "text");
            xmlWriter.WriteString(this.title);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteElementString(
                "published",
                this.published.ToString(
                    "o",
                    CultureInfo.InvariantCulture));

            xmlWriter.WriteElementString(
                "updated",
                this.updated.ToString(
                    "o",
                    CultureInfo.InvariantCulture));

            this.author.WriteTo(xmlWriter);

            this.WriteLinksTo(xmlWriter);

            this.content.WriteTo(xmlWriter, serializer);

            xmlWriter.WriteEndElement();
        }

        private void WriteLinksTo(XmlWriter xmlWriter)
        {
            foreach (var l in this.links)
                l.WriteTo(xmlWriter);
        }

        public static AtomEntry ReadFrom(XmlReader xmlReader)
        {
            return ReadFrom(
                xmlReader,
                new ConventionBasedSerializerOfComplexImmutableClasses());
        }

        public static AtomEntry ReadFrom(
            XmlReader xmlReader,
            IContentSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var navigator = new XPathDocument(xmlReader).CreateNavigator();

            var resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            var id = navigator
                .Select("/atom:entry/atom:id", resolver).Cast<XPathNavigator>()
                .Single().Value;
            var title = navigator
                .Select("/atom:entry/atom:title[@type = 'text']", resolver).Cast<XPathNavigator>()
                .Single().Value;
            var published = navigator
                .Select("/atom:entry/atom:published", resolver).Cast<XPathNavigator>()
                .Single().Value;
            var updated = navigator
                .Select("/atom:entry/atom:updated", resolver).Cast<XPathNavigator>()
                .Single().Value;
            var author = navigator
                .Select("/atom:entry/atom:author", resolver).Cast<XPathNavigator>()
                .Single().ReadSubtree();
            var content = navigator
                .Select("/atom:entry/atom:content[@type = 'application/xml']", resolver).Cast<XPathNavigator>()
                .Single().ReadSubtree();
            var links = navigator
                .Select("/atom:entry/atom:link", resolver).Cast<XPathNavigator>();

            return new AtomEntry(
                UuidIri.Parse(id),
                title,
                DateTimeOffset.Parse(published, CultureInfo.InvariantCulture),
                DateTimeOffset.Parse(updated, CultureInfo.InvariantCulture),
                AtomAuthor.ReadFrom(author),
                XmlAtomContent.ReadFrom(content, serializer),
                links.Select(x => AtomLink.ReadFrom(x.ReadSubtree())));
        }

        public AtomEntry AddLink(AtomLink newLink)
        {
            if (newLink == null)
                throw new ArgumentNullException("newLink");

            return this.WithLinks(this.links.Concat(new[] { newLink }));
        }

        public static AtomEntry Parse(string xml)
        {
            return Parse(
                xml,
                new ConventionBasedSerializerOfComplexImmutableClasses());
        }

        public static AtomEntry Parse(string xml, IContentSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var sr = new StringReader(xml);
            try
            {
                using (var r = XmlReader.Create(sr))
                {
                    sr = null;
                    return AtomEntry.ReadFrom(r);
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
