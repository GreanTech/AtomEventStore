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
        /// <seealso cref="Deserialize(XmlReader)" />
        void Serialize(XmlWriter xmlWriter, object value);

        /// <summary>
        /// Deserializes XML to an object.
        /// </summary>
        /// <param name="xmlReader">
        /// The <see cref="XlmReader" /> from which to read the XML.
        /// </param>
        /// <returns>
        /// An <see cref="XmlAtomContent" /> object containing the object read
        /// from <paramref name="xmlReader" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Note to implementers:</strong>
        /// </para>
        /// <para>
        /// The implementation should be able to correctly deserialize XML
        /// produced by the <see cref="Serialize(XmlWriter, object)" /> method.
        /// In other words, the implementation must be able to round-trip an
        /// object to and from XML.
        /// </para>
        /// <para>
        /// If <paramref name="xmlReader" /> contains invalid data, the
        /// implementation must throw an appropriate exception. It's considered
        /// an error to return <see langword="null" />.
        /// </para>
        /// </remarks>
        /// <seealso cref="Serialize(XmlWriter, object)" />
        XmlAtomContent Deserialize(XmlReader xmlReader);
    }
}
