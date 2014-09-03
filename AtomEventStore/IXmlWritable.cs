using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// An object that can be written as XML.
    /// </summary>
    public interface IXmlWritable
    {
        /// <summary>
        /// Writes the object to XML using the supplied
        /// <see cref="XmlWriter" /> and <see cref="IContentSerializer" />.
        /// </summary>
        /// <param name="xmlWriter">
        /// The <see cref="XmlWriter" /> with which the object should be
        /// written.
        /// </param>
        /// <param name="serializer">
        /// The <see cref="IContentSerializer" /> to use to serialize any
        /// custom content.
        /// </param>
        /// <remarks>
        /// <para>
        /// The <paramref name="serializer" /> may be ignored by the
        /// implementation if not necessary. However, clients must supply it.
        /// </para>
        /// </remarks>
        void WriteTo(XmlWriter xmlWriter, IContentSerializer serializer);
    }
}
