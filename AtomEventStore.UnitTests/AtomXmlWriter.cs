using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomXmlWriter<T> where T : IContentSerializer
    {
        private readonly T serializer;

        public AtomXmlWriter(T serializer)
        {
            this.serializer = serializer;
        }

        public string ToXml(AtomLink link)
        {
            return link
                .ToXmlString(
                    this.serializer,
                    new XmlWriterSettings { OmitXmlDeclaration = true })
                .Replace("xmlns=\"http://www.w3.org/2005/Atom\"", "");
        }

        public string ToXml(AtomEntry entry)
        {
            return entry
                .ToXmlString(
                    this.serializer,
                    new XmlWriterSettings { OmitXmlDeclaration = true })
                .Replace("xmlns=\"http://www.w3.org/2005/Atom\"", "");
        }
    }
}
