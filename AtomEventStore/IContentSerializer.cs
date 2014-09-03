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
        /// <summary>
        /// Serializes an object to XML and writes it to an
        /// <see cref="XmlWriter" />.
        /// </summary>
        /// <param name="xmlWriter">
        /// The <see cref="XmlWriter" /> to which the serialized object should
        /// be written.
        /// </param>
        /// <param name="value">
        /// The object to serialize to XML.
        /// </param>
        /// <remarks>
        /// <para>
        /// <strong>Note to implementers:</strong>
        /// </para>
        /// <para>
        /// The XML written by this method must be readable by the
        /// complementary implementation of
        /// <see cref="Deserialize(XmlReader)" />. In other words, the
        /// implementation must be able to round-trip an object to and from
        /// XML.
        /// </para>
        /// </remarks>
        void Serialize(XmlWriter xmlWriter, object value);

        XmlAtomContent Deserialize(XmlReader xmlReader);
    }
}
