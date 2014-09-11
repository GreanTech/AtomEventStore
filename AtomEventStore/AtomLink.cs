using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Represents an Atom link.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The AtomLink class represents a minimal set of required data in order
    /// to construct a valid Atom link according to the Atom Syndication
    /// Format specification at http://tools.ietf.org/html/rfc4287. Not all
    /// data elements or options defined by the specification are modelled by
    /// the AtomLink class. Instead, only those features and options required
    /// to implement AtomEventStore are included.
    /// </para>
    /// </remarks>
    public class AtomLink : IXmlWritable
    {
        private readonly string rel;
        private readonly Uri href;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomLink"/> class.
        /// </summary>
        /// <param name="rel">The relationship type.</param>
        /// <param name="href">The link address.</param>
        /// <remarks>
        /// <para>
        /// The value of <paramref name="rel" /> is subsequently available via
        /// the <see cref="Rel" /> property. The value of
        /// <paramref name="href" /> is subsequently available via the
        /// <see cref="Href" /> property.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="rel" />
        /// or
        /// <paramref name="href" /> is <see langword="null" />.
        /// </exception>
        public AtomLink(string rel, Uri href)
        {
            if (rel == null)
                throw new ArgumentNullException("rel");
            if (href == null)
                throw new ArgumentNullException("href");

            this.rel = rel;
            this.href = href;
        }

        /// <summary>
        /// Gets the relationship type.
        /// </summary>
        /// <value>
        /// The relationship type, as originally supplied via the constructor.
        /// </value>
        /// <seealso cref="AtomLink(string, Uri)" />
        public string Rel
        {
            get { return this.rel; }
        }

        /// <summary>
        /// Gets the link address.
        /// </summary>
        /// <value>
        /// The link address, as originally supplied via the constructor.
        /// </value>
        /// <seealso cref="AtomLink(string, Uri)" />
        public Uri Href
        {
            get { return this.href; }
        }

        /// <summary>
        /// Returns a new instance of <see cref="AtomLink" /> with thie
        /// supplied relationship type, but all other values held constant.
        /// </summary>
        /// <param name="newRel">The new relationship type.</param>
        /// <returns>
        /// A new instance of <see cref="AtomLink" /> with thie supplied
        /// relationship type, but all other values held constant.
        /// </returns>
        /// <seealso cref="Rel" />
        public AtomLink WithRel(string newRel)
        {
            return new AtomLink(newRel, this.href);
        }

        /// <summary>
        /// Returns a new instance of <see cref="AtomLink" /> with the supplied
        /// link address, but all other values held constant.
        /// </summary>
        /// <param name="newHref">The new link address.</param>
        /// <returns>
        /// A new instance of <see cref="AtomLink" /> with the supplied link
        /// address, but all other values held constant.
        /// </returns>
        /// <seealso cref="Href" />
        public AtomLink WithHref(Uri newHref)
        {
            return new AtomLink(this.rel, newHref);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to
        /// this instance.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object" /> to compare with this instance.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified <see cref="object" /> is
        /// another <see cref="AtomLink" /> instance with identical properties;
        /// otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Two <see cref="AtomLink" /> instances are considered to be equal if
        /// the have the same property values; that is, if they are
        /// structurally equal.
        /// </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            var other = obj as AtomLink;
            if (other != null)
                return object.Equals(this.rel, other.rel)
                    && object.Equals(this.href, other.href);

            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing
        /// algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return
                this.Rel.GetHashCode() ^
                this.Href.GetHashCode();
        }

        /// <summary>
        /// Writes an <see cref="AtomLink"/> to XML using the supplied
        /// <see cref="XmlWriter" />.
        /// </summary>
        /// <param name="xmlWriter">
        /// The <see cref="XmlWriter" /> with which the object should be
        /// written.
        /// </param>
        public void WriteTo(XmlWriter xmlWriter)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");

            xmlWriter.WriteStartElement("link", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteAttributeString("href", this.href.ToString());
            xmlWriter.WriteAttributeString("rel", this.rel);
            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes an <see cref="AtomLink"/>the object to XML using the
        /// supplied <see cref="XmlWriter" /> and
        /// <see cref="IContentSerializer" />.
        /// </summary>
        /// <param name="xmlWriter">
        /// The <see cref="XmlWriter" /> with which the object should be
        /// written.
        /// </param>
        /// <param name="serializer">
        /// The <see cref="IContentSerializer" /> to use to serialize any
        /// custom content. Ignore in this implementation.
        /// </param>
        /// <remarks>
        /// <para>
        /// <paramref name="serializer" /> is ignore in this implementation,
        /// because <see cref="AtomLink" /> has no custom content.
        /// </para>
        /// </remarks>
        public void WriteTo(XmlWriter xmlWriter, IContentSerializer serializer)
        {
            this.WriteTo(xmlWriter);
        }

        /// <summary>
        /// Parses the specified XML into an instance of
        /// <see cref="AtomLink" />.
        /// </summary>
        /// <param name="xml">
        /// A string of characters containing the XML representation of an Atom
        /// link.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="AtomLink" /> containing the data from
        /// the supplied <paramref name="xml" />.
        /// </returns>
        public static AtomLink Parse(string xml)
        {
            var sr = new StringReader(xml);
            try
            {
                using (var r = XmlReader.Create(sr))
                {
                    sr = null;
                    return AtomLink.ReadFrom(r);
                }
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
            }
        }

        /// <summary>
        /// Creates an <see cref="AtomLink" /> instance from XML.
        /// </summary>
        /// <param name="xmlReader">
        /// The <see cref="XmlReader" /> containing the XML representation of
        /// the Atom Link.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="AtomLink" /> containing the data from
        /// the XML representation of the Atom link contained in
        /// <paramref name="xmlReader" />.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// The supplied XML reader contains an atom:link element without an href attribute.
        /// or
        /// The supplied XML reader contains an atom:link element without a rel attribute.
        /// </exception>
        public static AtomLink ReadFrom(XmlReader xmlReader)
        {
            var navigator = new XPathDocument(xmlReader).CreateNavigator();

            var resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            var href = navigator
                .Select("/atom:link/@href", resolver)
                .Cast<XPathNavigator>()
                .Select(x => x.Value)
                .SingleOrDefault();
            if (href == null)
                throw new ArgumentException("The supplied XML reader contains an atom:link element without an href attribute. An atom:link element must have an href attribute.", "xmlReader");

            var rel = navigator
                .Select("/atom:link/@rel", resolver)
                .Cast<XPathNavigator>()
                .Select(x => x.Value)
                .SingleOrDefault();
            if (rel == null)
                throw new ArgumentException("The supplied XML reader contains an atom:link element without a rel attribute. An atom:link element must have a rel attribute.", "xmlReader");

            return new AtomLink(rel, new Uri(href, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Creates a self link.
        /// </summary>
        /// <param name="href">The address of the self link.</param>
        /// <returns>
        /// A new <see cref="AtomLink" /> instance with the <see cref="Rel" />
        /// value "self", and the <see cref="Href" /> value equal to
        /// <paramref name="href" />.
        /// </returns>
        public static AtomLink CreateSelfLink(Uri href)
        {
            return new AtomLink("self", href);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a self link.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if this instance is a self link; otherwise,
        /// <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        /// An <see cref="AtomLink" /> is considered a self link if its
        /// <see cref="Rel" /> value is "self".
        /// </para>
        /// </remarks>
        public bool IsSelfLink
        {
            get { return this.rel == "self"; }
        }

        /// <summary>
        /// Turns an <see cref="AtomLink" /> into a self link.
        /// </summary>
        /// <returns>
        /// A new instance af <see cref="AtomLink" /> with a <see cref="Rel" />
        /// value of "self", but all other properties held constant.
        /// </returns>
        public AtomLink ToSelfLink()
        {
            return this.WithRel("self");
        }

        /// <summary>
        /// Creates a via link.
        /// </summary>
        /// <param name="href">The address of the via link.</param>
        /// <returns>
        /// A new <see cref="AtomLink" /> instance with the <see cref="Rel" />
        /// value "via", and the <see cref="Href" /> value equal to
        /// <paramref name="href" />.
        /// </returns>
        public static AtomLink CreateViaLink(Uri href)
        {
            return new AtomLink("via", href);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a via link.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if this instance is a via link; otherwise,
        /// <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        /// An <see cref="AtomLink" /> is considered a via link if its
        /// <see cref="Rel" /> value is "via".
        /// </para>
        /// </remarks>
        public bool IsViaLink
        {
            get { return this.rel == "via"; }
        }

        /// <summary>
        /// Turns an <see cref="AtomLink" /> into a via link.
        /// </summary>
        /// <returns>
        /// A new instance af <see cref="AtomLink" /> with a <see cref="Rel" />
        /// value of "via", but all other properties held constant.
        /// </returns>
        public AtomLink ToViaLink()
        {
            return this.WithRel("via");
        }

        /// <summary>
        /// Creates a previous link.
        /// </summary>
        /// <param name="href">The address of the previous link.</param>
        /// <returns>
        /// A new <see cref="AtomLink" /> instance with the <see cref="Rel" />
        /// value "previous", and the <see cref="Href" /> value equal to
        /// <paramref name="href" />.
        /// </returns>
        public static AtomLink CreatePreviousLink(Uri uri)
        {
            return new AtomLink("previous", uri);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a previous link.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if this instance is a previous link;
        /// otherwise, <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        /// An <see cref="AtomLink" /> is considered a previous link if its
        /// <see cref="Rel" /> value is "previous".
        /// </para>
        /// </remarks>
        public bool IsPreviousLink
        {
            get { return this.rel == "previous"; }
        }

        /// <summary>
        /// Turns an <see cref="AtomLink" /> into a previous link.
        /// </summary>
        /// <returns>
        /// A new instance af <see cref="AtomLink" /> with a <see cref="Rel" />
        /// value of "previous", but all other properties held constant.
        /// </returns>
        public AtomLink ToPreviousLink()
        {
            return this.WithRel("previous");
        }

        /// <summary>
        /// Creates a next link.
        /// </summary>
        /// <param name="href">The address of the next link.</param>
        /// <returns>
        /// A new <see cref="AtomLink" /> instance with the <see cref="Rel" />
        /// value "next", and the <see cref="Href" /> value equal to
        /// <paramref name="href" />.
        /// </returns>
        public static AtomLink CreateNextLink(Uri uri)
        {
            return new AtomLink("next", uri);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a next link.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if this instance is a next link;
        /// otherwise, <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        /// An <see cref="AtomLink" /> is considered a next link if its
        /// <see cref="Rel" /> value is "next".
        /// </para>
        /// </remarks>
        public bool IsNextLink
        {
            get { return this.rel == "next"; }
        }

        /// <summary>
        /// Turns an <see cref="AtomLink" /> into a next link.
        /// </summary>
        /// <returns>
        /// A new instance af <see cref="AtomLink" /> with a <see cref="Rel" />
        /// value of "next", but all other properties held constant.
        /// </returns>
        public AtomLink ToNextLink()
        {
            return this.WithRel("next");
        }

        public static AtomLink CreateFirstLink(Uri uri)
        {
            return new AtomLink("first", uri);
        }

        public bool IsFirstLink
        {
            get { return this.rel == "first"; }
        }

        public AtomLink ToFirstLink()
        {
            return this.WithRel("first");
        }

        public static AtomLink CreateLastLink(Uri uri)
        {
            return new AtomLink("last", uri);
        }

        public bool IsLastLink
        {
            get { return this.rel == "last"; }
        }

        public AtomLink ToLastLink()
        {
            return this.WithRel("last");
        }
    }
}
