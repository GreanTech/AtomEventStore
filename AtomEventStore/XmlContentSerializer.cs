using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class XmlContentSerializer : IContentSerializer
    {
        public void Serialize(XmlWriter xmlWriter, object value)
        {
            throw new ArgumentNullException("xmlWriter");
        }

        public XmlAtomContent Deserialize(XmlReader xmlReader)
        {
            throw new ArgumentNullException("xmlReader");
        }
    }
}
