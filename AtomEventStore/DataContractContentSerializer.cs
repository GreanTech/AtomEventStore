using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// An Adapter that uses <see cref="DataContractSerializer" /> to implement
    /// <see cref="IContentSerializer" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A DataContractContentSerializer serializes and deserializes the
    /// contents of <see cref="XmlAtomContent" /> instances to and from XML
    /// using <see cref="DataContractSerializer" />.
    /// </para>
    /// </remarks>
    /// <seealso cref="XmlContentSerializer" />
    /// <seealso cref="IContentSerializer" />
    public class DataContractContentSerializer : IContentSerializer
    {
        private readonly ITypeResolver resolver;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataContractContentSerializer"/> class.
        /// </summary>
        /// <param name="resolver">
        /// An <see cref="ITypeResolver" /> used to resolve XML names to
        /// <see cref="Type" /> instances, used when deserializing XML to
        /// objects.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="resolver" /> is <see langword="null" />.
        /// </exception>
        public DataContractContentSerializer(ITypeResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException("resolver");

            this.resolver = resolver;
        }

        public void Serialize(XmlWriter xmlWriter, object value)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");
            if (value == null)
                throw new ArgumentNullException("value");

            var serializer = new DataContractSerializer(value.GetType());
            serializer.WriteObject(xmlWriter, value);
        }

        public XmlAtomContent Deserialize(XmlReader xmlReader)
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");

            xmlReader.MoveToContent();
            var localName = xmlReader.Name;
            var xmlNamespace = xmlReader.NamespaceURI;
            var type = this.resolver.Resolve(localName, xmlNamespace);

            var serializer = new DataContractSerializer(type);
            var value = serializer.ReadObject(xmlReader);

            return new XmlAtomContent(value);
        }
    }
}
