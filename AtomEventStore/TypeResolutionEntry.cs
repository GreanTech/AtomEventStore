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

        public TypeResolutionEntry(
            string xmlNamespace)
        {
            this.xmlNamespace = xmlNamespace;
        }

        public string XmlNamespace
        {
            get { return this.xmlNamespace; }
        }
    }
}
