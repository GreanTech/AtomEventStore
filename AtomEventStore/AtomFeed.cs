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
    /// <summary>
    /// Represents an Atom Feed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The AtomFeed class represents a minimal set of required data in order
    /// to construct a valid Atom Feed according to the Atom Syndication Format
    /// specification at http://tools.ietf.org/html/rfc4287. Not all data
    /// elements or options defined by the specification are modelled by the
    /// AtomFeed class. Instead, only those features and options required to
    /// implement AtomEventStore are included.
    /// </para>
    /// </remarks>
    public class AtomFeed : IXmlWritable
    {
        private readonly UuidIri id;
        private readonly string title;
        private readonly DateTimeOffset updated;
        private readonly AtomAuthor author;
        private readonly IEnumerable<AtomEntry> entries;
        private readonly IEnumerable<AtomLink> links;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomFeed"/> class.
        /// </summary>
        /// <param name="id">The ID of the Atom Feed.</param>
        /// <param name="title">The title of the Atom Feed.</param>
        /// <param name="updated">
        /// The date and time the Atom Feed was last updated.
        /// </param>
        /// <param name="author">The author of the Atom Feed.</param>
        /// <param name="entries">The entries of the Atom Feed.</param>
        /// <param name="links">The links of the Atom Feed itself.</param>
        /// <remarks>
        /// <para>
        /// All values passed into this constructor are subsequently available
        /// as properties on the instance.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="title" />
        /// or
        /// <paramref name="author" />
        /// or
        /// <paramref name="entries" />
        /// or
        /// <paramref name="links" />
        /// is <see langword="null" />.
        /// </exception>
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

        /// <summary>
        /// Gets the ID of the Atom Feed.
        /// </summary>
        /// <value>
        /// The ID of the Atom Feed as originally supplied via the constructor.
        /// </value>
        /// <seealso cref="AtomFeed(UuidIri, string, DateTimeOffset, AtomAuthor, IEnumerable{AtomEnty}, IEnumerable{AtomLink})" />
        public UuidIri Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the title of the Atom Feed.
        /// </summary>
        /// <value>
        /// The title of the Atom Feed as originally supplied via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomFeed(UuidIri, string, DateTimeOffset, AtomAuthor, IEnumerable{AtomEnty}, IEnumerable{AtomLink})" />
        public string Title
        {
            get { return this.title; }
        }

        public DateTimeOffset Updated
        {
            get { return this.updated; }
        }

        /// <summary>
        /// Gets the author of the Atom Feed.
        /// </summary>
        /// <value>
        /// The author of the Atom Feed as originally supplied via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomFeed(UuidIri, string, DateTimeOffset, AtomAuthor, IEnumerable{AtomEnty}, IEnumerable{AtomLink})" />
        public AtomAuthor Author
        {
            get { return this.author; }
        }

        /// <summary>
        /// Gets the entries of the Atom Feed.
        /// </summary>
        /// <value>
        /// The entries of the Atom Feed as originally supplied via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomFeed(UuidIri, string, DateTimeOffset, AtomAuthor, IEnumerable{AtomEnty}, IEnumerable{AtomLink})" />
        public IEnumerable<AtomEntry> Entries
        {
            get { return this.entries; }
        }

        /// <summary>
        /// Gets the links of the Atom Feed.
        /// </summary>
        /// <value>
        /// The links of the Atom Feed as originally supplied via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomFeed(UuidIri, string, DateTimeOffset, AtomAuthor, IEnumerable{AtomEnty}, IEnumerable{AtomLink})" />
        public IEnumerable<AtomLink> Links
        {
            get { return this.links; }
        }

        /// <summary>
        /// Returns a new instance of <see cref="AtomFeed" /> with the supplied
        /// title, but all other values held constant.
        /// </summary>
        /// <param name="newTitle">The new title.</param>
        /// <returns>
        /// A new instance of <see cref="AtomFeed" /> with the supplied title,
        /// but all other values held constant.
        /// </returns>
        /// <seealso cref="Title" />
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

        /// <summary>
        /// Returns a new instance of <see cref="AtomFeed" /> with the supplied
        /// update date and time, but all other values held constant.
        /// </summary>
        /// <param name="newUpdated">The new update date and time.</param>
        /// <returns>
        /// A new instance of <see cref="AtomFeed" /> with the supplied
        /// update date and time, but all other values held constant.
        /// </returns>
        /// <seealso cref="Updated" />
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

        /// <summary>
        /// Returns a new instance of <see cref="AtomFeed" /> with the supplied
        /// author, but all other values held constant.
        /// </summary>
        /// <param name="newAuthor">The new author.</param>
        /// <returns>
        /// A new instance of <see cref="AtomFeed" /> with the supplied author,
        /// but all other values held constant.
        /// </returns>
        /// <seealso cref="Author" />
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

        /// <summary>
        /// Returns a new instance of <see cref="AtomFeed" /> with the suplied
        /// entries, but all other values held constant.
        /// </summary>
        /// <param name="newEntries">The new entries.</param>
        /// <returns>
        /// A new instance of <see cref="AtomFeed" /> with the suplied entries,
        /// but all other values held constant.
        /// </returns>
        /// <see cref="Entries" />
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

        /// <summary>
        /// Returns a new instance of <see cref="AtomFeed" /> with the supplied
        /// links, but all other vlaues held constant.
        /// </summary>
        /// <param name="newLinks">The new links.</param>
        /// <returns>
        /// A new instance of <see cref="AtomFeed" /> with the supplied links,
        /// but all other vlaues held constant.
        /// </returns>
        /// <see cref="Links" />
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

        /// <summary>
        /// Writes the object to XML using the supplied
        /// <see cref="XmlWriter" /> and <see cref="IContentSerializer" />.
        /// </summary>
        /// <param name="xmlWriter">
        /// The <see cref="XmlWriter" /> with which the object should be
        /// written.
        /// </param>
        /// <param name="serializer">
        /// The <see cref="IContentSerializer" /> to use to serialize any
        /// custom content.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="xmlWriter" /> is <see langword="null" />.
        /// </exception>
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
                    return AtomFeed.ReadFrom(r, serializer);
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
