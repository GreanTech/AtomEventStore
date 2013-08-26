using System;
using System.Xml;

namespace Grean.AtomEventStore
{
    public interface IContentSerializer
    {
        void Serialize(XmlWriter xmlWriter, object value);

        XmlAtomContent Deserialize(XmlReader xmlReader);
    }
}
