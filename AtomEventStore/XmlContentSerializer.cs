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
            throw new ArgumentNullException("xmlReader");
        }
    }
}
