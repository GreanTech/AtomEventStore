using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class DataContractContentSerializer : IContentSerializer
    {
        private readonly ITypeResolver resolver;

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
