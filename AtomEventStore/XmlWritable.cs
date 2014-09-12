using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Extension methods related to <see cref="IXmlWritable" />.
    /// </summary>
    public static class XmlWritable
    {
        /// <summary>
        /// Converts an <see cref="IXmlWritable" /> to an XML string.
        /// </summary>
        /// <param name="xmlWritable">
        /// The object that can be converted to XML.
        /// </param>
        /// <param name="serializer">
        /// A serializer that can serialize custom content, in case the object
        /// contains custom content.
        /// </param>
        /// <returns>
        /// A string of characters containing XML corresponding to the data in
        /// the object.
        /// </returns>
        /// <seealso cref="ToXmlString(IXmlWritable, IContentSerializer, XmlWriterSettings)" />
        public static string ToXmlString(
            this IXmlWritable xmlWritable,
            IContentSerializer serializer)
        {
            return xmlWritable.ToXmlString(serializer, new XmlWriterSettings());
        }

        /// <summary>
        /// Converts an <see cref="IXmlWritable" /> to an XML string.
        /// </summary>
        /// <param name="xmlWritable">
        /// The object that can be converted to XML.
        /// </param>
        /// <param name="serializer">
        /// A serializer that can serialize custom content, in case the object
        /// contains custom content.
        /// </param>
        /// <param name="settings">
        /// Settings that control how the XML is formatted.
        /// </param>
        /// <returns>
        /// A string of characters containing XML corresponding to the data in
        /// the object.
        /// </returns>
        public static string ToXmlString(
            this IXmlWritable xmlWritable,
            IContentSerializer serializer,
            XmlWriterSettings settings)
        {
            if (xmlWritable == null)
                throw new ArgumentNullException("xmlWritable");

            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb, settings))
            {
                xmlWritable.WriteTo(w, serializer);
                w.Flush();
                return sb.ToString();
            }
        }
    }
}
