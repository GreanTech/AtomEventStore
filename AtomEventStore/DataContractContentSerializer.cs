using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class DataContractContentSerializer : IContentSerializer
    {
        public void Serialize(XmlWriter xmlWriter, object value)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");
            if (value == null)
                throw new ArgumentNullException("value");
        }

        public XmlAtomContent Deserialize(XmlReader xmlReader)
        {
            throw new ArgumentNullException("xmlReader");
        }
    }
}
