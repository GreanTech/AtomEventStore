using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Represents the content element in an Atom Entry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The XmlAtomContent class represents a minimal set of required data in
    /// order to construct a valid Atom content element according to the Atom
    /// Syndication Format specification at http://tools.ietf.org/html/rfc4287.
    /// Not all data elements or options defined by the specification are
    /// modelled by the XmlAtomContent class. Instead, only those features and
    /// options required to implement AtomEventStore are included.
    /// </para>
    /// </remarks>
    public class XmlAtomContent : IXmlWritable
    {
        private readonly object item;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlAtomContent"/>
        /// class.
        /// </summary>
        /// <param name="item">The actual content of the element.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="item" /> is subsequently available to clients via
        /// the <see cref="Item" /> property.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="item" /> is <see langword="null" />.
        /// </exception>
        public XmlAtomContent(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            this.item = item;
        }

        /// <summary>
        /// Gets the content item.
        /// </summary>
        /// <value>
        /// The content item, as originally supplied via the constructor.
        /// </value>
        /// <seealso cref="XmlAtomContent(object)" />
        public object Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Returns a new instance of <see cref="XmlAtomContent" /> with the
        /// supplied content item, but all other properties held constant.
        /// </summary>
        /// <param name="newName">The new content item.</param>
        /// <returns>
        /// A new instance of <see cref="XmlAtomContent" /> with the supplied
        /// name, but all other properties held constant.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method mostly exists to make <see cref="XmlAtomContent" />
        /// consistent with the other Atom classes with similar methods. The
        /// method also exists for future compatibility reasons. However,
        /// currently, since XmlAtomContent only contains the saingle
        /// <see cref="Item" /> property, there are no other properties to hold
        /// constant. However, clients can use this method as a more robust,
        /// forward-compatible way of copying an XmlAtomContent instance with a
        /// new content item.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "While the documentation of this CA warning mostly states that you can suppress this warning for already shipped code, as it would be a breaking change to address it, I'm taking the reverse position: making it static now would mean that it'd be a breaking change to make it an instance method later. All these 'With' methods are, in their nature, instance methods. The only reason the 'this' keyword isn't used here is because there's only a single field on the class, but this may change in the future.")]
        public XmlAtomContent WithItem(object newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException("newItem");

            return new XmlAtomContent(newItem);
        }

        /// <summary>
        /// Determines whether the supplied <see cref="object" />, is equal to
        /// this instance.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object" /> to compare with this instance.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified <see cref="object" /> is
        /// equal to this instance; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="obj" /> is another <see cref="XmlAtomContent" />
        /// instance, this implementation uses the <see cref="Item" />
        /// properties' own implementation of Equals to compare the items.
        /// </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            var other = obj as XmlAtomContent;
            if (other != null)
                return object.Equals(this.item, other.item);

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
            return this.item.GetHashCode();
        }

        public void WriteTo(XmlWriter xmlWriter, IContentSerializer serializer)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            xmlWriter.WriteStartElement("content", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteAttributeString("type", "application/xml");

            serializer.Serialize(xmlWriter, this.item);

            xmlWriter.WriteEndElement();
        }

        public static XmlAtomContent Parse(
            string xml,
            IContentSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var sr = new StringReader(xml);
            try
            {
                using (var r = XmlReader.Create(sr))
                {
                    sr = null;
                    return XmlAtomContent.ReadFrom(r, serializer);
                }
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
            }
        }

        public static XmlAtomContent ReadFrom(
            XmlReader xmlReader,
            IContentSerializer serializer)
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            GuardContentElement(xmlReader);

            xmlReader.Read();

            return serializer.Deserialize(xmlReader);
        }

        private static void GuardContentElement(XmlReader xmlReader)
        {
            xmlReader.MoveToContent();

            if (xmlReader.LocalName != "content")
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "The containing XML element of an Atom XML content entry must have the local name \"content\", but the name was \"{0}\". The name comparison is case-sensitive.",
                        xmlReader.LocalName),
                    "xmlReader");
            if (xmlReader.NamespaceURI != "http://www.w3.org/2005/Atom")
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "The containing XML element of an Atom XML content entry must have the XML namespace \"http://www.w3.org/2005/Atom\", but the XML namespace was \"{0}\". The namespace comparison is case-sensitive.",
                        xmlReader.NamespaceURI),
                    "xmlReader");

            if (!xmlReader.MoveToAttribute("type"))
                throw new ArgumentException(
                    "The containing XML element of an Atom XML content entry must have a \"type\" attribute, but none was found. The attribute name comparison is case-sensitive.",
                    "xmlReader");
            if (xmlReader.Value != "application/xml")
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "The containing XML element of an Atom XML content entry must have a \"type\" attribute with the value \"application/xml\", but the value was \"{0}\". The value comparison is case-sensitive.",
                        xmlReader.Value),
                    "xmlReader");
        }
    }
}
