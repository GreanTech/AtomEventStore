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
    /// Represents an Author of an Atom document.
    /// </summary>
    public class AtomAuthor : IXmlWritable
    {
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomAuthor"/> class.
        /// </summary>
        /// <param name="name">The name of the Author.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="name" /> argument is subsequently available via
        /// the <see cref="Name" /> property.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="name" /> is <see langword="null" />.
        /// </exception>
        public AtomAuthor(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
        }

        /// <summary>
        /// Gets the name of the author.
        /// </summary>
        /// <value>
        /// The name of the author, as provided via the constructor.
        /// </value>
        /// <seealso cref="AtomAuthor(string)" />
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Returns a new instance of <see cref="AtomAuthor" /> with the
        /// supplied name, but all other properties held constant.
        /// </summary>
        /// <param name="newName">The new name.</param>
        /// <returns>
        /// A new instance of <see cref="AtomAuthor" /> with the supplied name,
        /// but all other properties held constant.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method mostly exists to make <see cref="AtomAuthor" />
        /// consistent with the other Atom classes with similar methods. The
        /// method also exists for future compatibility reasons. However,
        /// currently, since AtomAuthor only contains the saingle
        /// <see cref="Name" /> property, there are no other properties to hold
        /// constant. However, clients can use this method as a more robust,
        /// forward-compatible way of copying an AtomAuthor instance with a new
        /// name.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "While the documentation of this CA warning mostly states that you can suppress this warning for already shipped code, as it would be a breaking change to address it, I'm taking the reverse position: making it static now would mean that it'd be a breaking change to make it an instance method later. All these 'With' methods are, in their nature, instance methods. The only reason the 'this' keyword isn't used here is because there's only a single field on the class, but this may change in the future.")]
        public AtomAuthor WithName(string newName)
        {
            return new AtomAuthor(newName);
        }

        public override bool Equals(object obj)
        {
            var other = obj as AtomAuthor;
            if (other != null)
                return object.Equals(this.name, other.name);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        public void WriteTo(XmlWriter xmlWriter)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");

            xmlWriter.WriteStartElement("author", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteElementString("name", this.name);
            xmlWriter.WriteEndElement();
        }

        public void WriteTo(XmlWriter xmlWriter, IContentSerializer serializer)
        {
            this.WriteTo(xmlWriter);
        }

        public static AtomAuthor ReadFrom(XmlReader xmlReader)
        {
            var navigator = new XPathDocument(xmlReader).CreateNavigator();

            var resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            var name = navigator
                .Select("/atom:author/atom:name", resolver).Cast<XPathNavigator>()
                .Single().Value;

            return new AtomAuthor(name);
        }

        public static AtomAuthor Parse(string xml)
        {
            var sr = new StringReader(xml);
            try
            {
                using (var r = XmlReader.Create(sr))
                {
                    sr = null;
                    return AtomAuthor.ReadFrom(r);
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
