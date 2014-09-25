using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Represents an Entry in a <see cref="TypeResolutionTable"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The TypeResolutionEntry class represents a set of required data in
    /// order to construct a valid <see cref="TypeResolutionTable"/>.
    /// </para>
    /// </remarks>
    public class TypeResolutionEntry
    {
        private readonly string xmlNamespace;
        private readonly string localName;
        private readonly Type resolution;

        public TypeResolutionEntry(
            string xmlNamespace,
            string localName,
            Type resolution)
        {
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");
            if (localName == null)
                throw new ArgumentNullException("localName");
            if (resolution == null)
                throw new ArgumentNullException("resolution");

            this.xmlNamespace = xmlNamespace;
            this.localName = localName;
            this.resolution = resolution;
        }

        public string XmlNamespace
        {
            get { return this.xmlNamespace; }
        }

        public string LocalName
        {
            get { return this.localName; }
        }

        public Type Resolution
        {
            get { return this.resolution; }
        }
    }
}
