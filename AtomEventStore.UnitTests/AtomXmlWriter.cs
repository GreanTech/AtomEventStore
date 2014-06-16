using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomXmlWriter
    {
        public string ToXml(AtomLink link)
        {
            return link
                .ToXmlString(
                    new ConventionBasedSerializerOfComplexImmutableClasses(),
                    new XmlWriterSettings { OmitXmlDeclaration = true })
                .Replace("xmlns=\"http://www.w3.org/2005/Atom\"", "");
        }

        public string ToXml(AtomEntry entry)
        {
            return entry
                .ToXmlString(
                    new ConventionBasedSerializerOfComplexImmutableClasses(),
                    new XmlWriterSettings { OmitXmlDeclaration = true })
                .Replace("xmlns=\"http://www.w3.org/2005/Atom\"", "");
        }
    }
}
