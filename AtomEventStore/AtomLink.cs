using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Grean.AtomEventStore
{
    public class AtomLink : IXmlWritable
    {
        private readonly string rel;
        private readonly Uri href;

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

        public static AtomLink Parse(string xml)
        {
            using (var sr = new StringReader(xml))
            using (var r = XmlReader.Create(sr))
                return AtomLink.ReadFrom(r);
        }

        public static AtomLink ReadFrom(XmlReader xmlReader)
        {
            var navigator = new XPathDocument(xmlReader).CreateNavigator();

            var resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            var href = navigator
                .Select("/atom:link/@href", resolver).Cast<XPathNavigator>()
                .Single().Value;
            var rel = navigator
                .Select("/atom:link/@rel", resolver).Cast<XPathNavigator>()
                .Single().Value;

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
    }
}
