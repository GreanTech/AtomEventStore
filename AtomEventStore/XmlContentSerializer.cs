using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// An Adapter that uses <see cref="XmlSerializer" /> to implement
    /// <see cref="IContentSerializer" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An XmlContentSerializer serializes and deserializes the contents of
    /// <see cref="XmlAtomContent" /> instances to and from XML using
    /// <see cref="XmlSerializer" />.
    /// </para>
    /// </remarks>
    /// <seealso cref="DataContractContentSerializer" />
    /// <seealso cref="IContentSerializer" />
    public class XmlContentSerializer : IContentSerializer
    {
        private readonly ITypeResolver resolver;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="XmlContentSerializer"/> class.
        /// </summary>
        /// <param name="resolver">
        /// An <see cref="ITypeResolver" /> used to resolve XML names to
        /// <see cref="Type" /> instances, used when deserializing XML to
        /// objects.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="resolver" /> is <see langword="null" />.
        /// </exception>
        public XmlContentSerializer(ITypeResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException("resolver");

            this.resolver = resolver;
        }

        /// <summary>
        /// Serializes an object to XML and writes it to an
        /// <see cref="XmlWriter" />.
        /// </summary>
        /// <param name="xmlWriter">
        /// The <see cref="XmlWriter" /> to which the serialized object should
        /// be written.
        /// </param>
        /// <param name="value">The object to serialize to XML.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="xmlWriter" /> or <paramref name="value" /> is
        /// <see langword="null" />.
        /// </exception>
        /// <seealso cref="Deserialize(XmlReader)" />
        public void Serialize(XmlWriter xmlWriter, object value)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");
            if (value == null)
                throw new ArgumentNullException("value");
            
            var serializer = new XmlSerializer(value.GetType());
            serializer.Serialize(xmlWriter, value);
        }

        /// <summary>
        /// Deserializes XML to an object.
        /// </summary>
        /// <param name="xmlReader">
        /// The <see cref="XmlReader" /> from which to read the XML.
        /// </param>
        /// <returns>
        /// An <see cref="XmlAtomContent" /> object containing the object read
        /// from <paramref name="xmlReader" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The Deserialize method uses the contained
        /// <see cref="ITypeResolver" /> to identify the type of object being
        /// deserialized, based on the local name and XML namespace in the XML
        /// read from <paramref name="xmlReader" />.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="xmlReader" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="Serialize(XmlWriter, object)" />
        public XmlAtomContent Deserialize(XmlReader xmlReader)
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");

            xmlReader.MoveToContent();
            var localName = xmlReader.Name;
            var xmlNamespace = xmlReader.NamespaceURI;
            var type = this.resolver.Resolve(localName, xmlNamespace);

            var serializer = new XmlSerializer(type);
            var value = serializer.Deserialize(xmlReader);

            return new XmlAtomContent(value);
        }

        public static ITypeResolver CreateTypeResolver(
            Assembly assemblyToScanForEvents)
        {
            if (assemblyToScanForEvents == null)
                throw new ArgumentNullException("assemblyToScanForEvents");

            throw new NotImplementedException();
        }
    }
}
