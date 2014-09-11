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

        public string Rel
        {
            get { return this.rel; }
        }

        public Uri Href
        {
            get { return this.href; }
        }

        public AtomLink WithRel(string newRel)
        {
            return new AtomLink(newRel, this.href);
        }

        public AtomLink WithHref(Uri newHref)
        {
            return new AtomLink(this.rel, newHref);
        }

        public override bool Equals(object obj)
        {
            var other = obj as AtomLink;
            if (other != null)
                return object.Equals(this.rel, other.rel)
                    && object.Equals(this.href, other.href);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return
                this.Rel.GetHashCode() ^
                this.Href.GetHashCode();
        }

        public void WriteTo(XmlWriter xmlWriter)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");

            xmlWriter.WriteStartElement("link", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteAttributeString("href", this.href.ToString());
            xmlWriter.WriteAttributeString("rel", this.rel);
            xmlWriter.WriteEndElement();
        }

        public void WriteTo(XmlWriter xmlWriter, IContentSerializer serializer)
        {
            this.WriteTo(xmlWriter);
        }

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

        public static AtomLink CreateSelfLink(Uri href)
        {
            return new AtomLink("self", href);
        }

        public bool IsSelfLink
        {
            get { return this.rel == "self"; }
        }

        public AtomLink ToSelfLink()
        {
            return this.WithRel("self");
        }

        public static AtomLink CreateViaLink(Uri href)
        {
            return new AtomLink("via", href);
        }

        public bool IsViaLink
        {
            get { return this.rel == "via"; }
        }

        public AtomLink ToViaLink()
        {
            return this.WithRel("via");
        }

        public static AtomLink CreatePreviousLink(Uri uri)
        {
            return new AtomLink("previous", uri);
        }

        public bool IsPreviousLink
        {
            get { return this.rel == "previous"; }
        }

        public AtomLink ToPreviousLink()
        {
            return this.WithRel("previous");
        }

        public static AtomLink CreateNextLink(Uri uri)
        {
            return new AtomLink("next", uri);
        }

        public bool IsNextLink
        {
            get { return this.rel == "next"; }
        }

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
