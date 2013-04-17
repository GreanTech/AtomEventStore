using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class XmlAtomContent
    {
        private readonly object item;
        private readonly Type itemType;
        private readonly string itemXmlElement;
        private readonly string itemXmlNamespace;

        public XmlAtomContent(object item)
        {
            this.item = item;
            this.itemType = item.GetType();
            this.itemXmlElement = Xmlify(this.itemType.Name);
            this.itemXmlNamespace = Urnify(this.itemType.Namespace);
        }

        public object Item
        {
            get { return this.item; }
        }

        public XmlAtomContent WithItem(object newItem)
        {
            return new XmlAtomContent(newItem);
        }

        public override bool Equals(object obj)
        {
            var other = obj as XmlAtomContent;
            if (other != null)
                return object.Equals(this.item, other.item);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.item.GetHashCode();
        }

        internal void WriteTo(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("content");
            xmlWriter.WriteAttributeString("type", "application/xml");

            xmlWriter.WriteStartElement(this.itemXmlElement, this.itemXmlNamespace);

            this.WriteItemTo(xmlWriter);

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
        }

        private void WriteItemTo(XmlWriter xmlWriter)
        {
            foreach (var p in this.itemType.GetProperties())
            {
                var localName = Xmlify(p.Name);
                var value = p.GetValue(this.item).ToString();
                xmlWriter.WriteElementString(localName, value);
            }
        }

        private static string Urnify(string text)
        {
            return "urn:" + string.Join(":", text.Split('.').Select(Xmlify));
        }

        private static string Xmlify(string text)
        {
            return text
                .Take(1).Select(Char.ToLower).Concat(text.Skip(1))
                .Aggregate("", (s, c) => Char.IsUpper(c) ? s + "-" + c : s + c)
                .ToLower();
        }
    }
}
