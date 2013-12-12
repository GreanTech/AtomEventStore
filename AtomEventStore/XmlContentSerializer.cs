using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Grean.AtomEventStore
{
    public class XmlContentSerializer : IContentSerializer
    {
        private readonly ITypeResolver resolver;

        public XmlContentSerializer(ITypeResolver resolver)
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
            
            var serializer = new XmlSerializer(value.GetType());
            serializer.Serialize(xmlWriter, value);
        }

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
    }
}
