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
    /// Represents an Entry in an Atom Feed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The AtomEntry class represents a minimal set of required data in order
    /// to construct a valid Atom Entry according to the Atom Syndication
    /// Format specification at http://tools.ietf.org/html/rfc4287. Not all
    /// data elements or options defined by the specification are modelled by
    /// the AtomEntry class. Instead, only those features and options required
    /// to implement AtomEventStore are included.
    /// </para>
    /// </remarks>
    public class AtomEntry : IXmlWritable
    {
        private readonly UuidIri id;
        private readonly string title;
        private readonly DateTimeOffset published;
        private readonly DateTimeOffset updated;
        private readonly AtomAuthor author;
        private readonly XmlAtomContent content;
        private readonly IEnumerable<AtomLink> links;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEntry"/> class.
        /// </summary>
        /// <param name="id">The ID of the entry.</param>
        /// <param name="title">The title of the entry.</param>
        /// <param name="published">The date and time of publication.</param>
        /// <param name="updated">
        /// The date and time the entry was last updated.
        /// </param>
        /// <param name="author">The author of the entry.</param>
        /// <param name="content">The content of the entry.</param>
        /// <param name="links">The links of the entry.</param>
        /// <remarks>
        /// <para>
        /// The constructor arguments are subsequently available on the object
        /// as properties.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="title" />
        /// or
        /// <paramref name="author" />
        /// or
        /// <paramref name="content" />
        /// or
        /// <paramref name="content" /> is <see langword="null" />.
        /// </exception>
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

        /// <summary>
        /// Gets the ID of the Atom Entry.
        /// </summary>
        /// <value>
        /// The ID of the Atom Entry, as originally provided via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomEnty(UuidIri, string, DateTimeOffset, DateTimeOffset, AtomAuthor, XmlAtomContent, IEnumerable{AtomLink})" />
        public UuidIri Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the title of the Atom Entry.
        /// </summary>
        /// <value>
        /// The title of the Atom Entry, as originally provided via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomEnty(UuidIri, string, DateTimeOffset, DateTimeOffset, AtomAuthor, XmlAtomContent, IEnumerable{AtomLink})" />
        public string Title
        {
            get { return this.title; }
        }

        /// <summary>
        /// Gets the publication date and time of the Atom Entry.
        /// </summary>
        /// <value>
        /// The publication date and time of the Atom Entry, as originally
        /// provided via the constructor.
        /// </value>
        /// <seealso cref="AtomEnty(UuidIri, string, DateTimeOffset, DateTimeOffset, AtomAuthor, XmlAtomContent, IEnumerable{AtomLink})" />
        public DateTimeOffset Published
        {
            get { return this.published; }
        }

        /// <summary>
        /// Gets the update date and time of the Atom Entry.
        /// </summary>
        /// <value>
        /// The update date and time of the Atom Entry, as originally provided
        /// via the constructor.
        /// </value>
        /// <seealso cref="AtomEnty(UuidIri, string, DateTimeOffset, DateTimeOffset, AtomAuthor, XmlAtomContent, IEnumerable{AtomLink})" />
        public DateTimeOffset Updated
        {
            get { return this.updated; }
        }

        /// <summary>
        /// Gets the author of the Atom Entry.
        /// </summary>
        /// <value>
        /// The author of the Atom Entry, as originally provided via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomEnty(UuidIri, string, DateTimeOffset, DateTimeOffset, AtomAuthor, XmlAtomContent, IEnumerable{AtomLink})" />
        public AtomAuthor Author
        {
            get { return this.author; }
        }

        /// <summary>
        /// Gets the content of the Atom Entry.
        /// </summary>
        /// <value>
        /// The content of the Atom Entry, as originally provided via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomEnty(UuidIri, string, DateTimeOffset, DateTimeOffset, AtomAuthor, XmlAtomContent, IEnumerable{AtomLink})" />
        public XmlAtomContent Content
        {
            get { return this.content; }
        }

        /// <summary>
        /// Gets the links of the Atom Entry.
        /// </summary>
        /// <value>
        /// The links of the Atom Entry, as originally provided via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomEnty(UuidIri, string, DateTimeOffset, DateTimeOffset, AtomAuthor, XmlAtomContent, IEnumerable{AtomLink})" />
        public IEnumerable<AtomLink> Links
        {
            get { return this.links; }
        }

        /// <summary>
        /// Returns a new instance of <see cref="AtomEntry" /> with the
        /// supplied title, but all other values held constant.
        /// </summary>
        /// <param name="newTitle">The new title.</param>
        /// <returns>
        /// A new instance of <see cref="AtomEntry" /> with the supplied title,
        /// but all other values held constant.
        /// </returns>
        /// <seealso cref="Title" />
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

        /// <summary>
        /// Returns a new instance of <see cref="AtomEntry" /> with the
        /// supplied update date and time, but all other values held constant.
        /// </summary>
        /// <param name="newUpdated">The new update date and time.</param>
        /// <returns>
        /// A new instance of <see cref="AtomEntry" /> with the supplied update
        /// date and time, but all other values held constant.
        /// </returns>
        /// <seealso cref="Updated" />
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

        /// <summary>
        /// Returns a new instance of <see cref="AtomEntry" /> with the
        /// supplied author, but all other values held constant.
        /// </summary>
        /// <param name="newAuthor">The new author.</param>
        /// <returns>
        /// A new instance of <see cref="AtomEntry" /> with the supplied
        /// author, but all other values held constant.
        /// </returns>
        /// <seealso cref="Author" />
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

        /// <summary>
        /// Returns a new instance of <see cref="AtomEntry" /> with the
        /// supplied content, but all other values held constant.
        /// </summary>
        /// <param name="newContent">The new content.</param>
        /// <returns>
        /// A new instance of <see cref="AtomEntry" /> with the supplied
        /// content, but all other values held constant.
        /// </returns>
        /// <seealso cref="Content" />
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

        /// <summary>
        /// Returns a new instance of <see cref="AtomEntry" /> with the
        /// supplied links, but all other values held constant.
        /// </summary>
        /// <param name="newLinks">The new links.</param>
        /// <returns>
        /// A new instance of <see cref="AtomEntry" /> with the supplied links,
        /// but all other values held constant.
        /// </returns>
        /// <seealso cref="Links" />
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
                    return AtomEntry.ReadFrom(r, serializer);
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
