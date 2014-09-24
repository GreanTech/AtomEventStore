using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class TypeResolutionEntry
    {
        private readonly string xmlNamespace;
        private readonly string localName;

        public TypeResolutionEntry(
            string xmlNamespace,
            string localName)
        {
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");
            if (localName == null)
                throw new ArgumentNullException("localName");

            this.xmlNamespace = xmlNamespace;
            this.localName = localName;
        }

        public string XmlNamespace
        {
            get { return this.xmlNamespace; }
        }

        public string LocalName
        {
            get { return this.localName; }
        }
    }
}
