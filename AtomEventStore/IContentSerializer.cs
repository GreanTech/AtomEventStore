using System;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Serializes and deserializes object to XML.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is used by AtomEventStore to serialize the contents of
    /// <see cref="XmlAtomContent" /> instances to and from XML. This enables
    /// users of AtomEventStore to pick the serialization mechanism that works
    /// best for their needs.
    /// </para>
    /// </remarks>
    /// <seealso cref="XmlContentSerializer" />
    /// <seealso cref="DataContractContentSerializer" />
    public interface IContentSerializer
    {
        void Serialize(XmlWriter xmlWriter, object value);

        XmlAtomContent Deserialize(XmlReader xmlReader);
    }
}
