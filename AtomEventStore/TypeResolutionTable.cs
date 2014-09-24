using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class TypeResolutionTable : ITypeResolver
    {
        private readonly IEnumerable<TypeResolutionEntry> entries;

        public TypeResolutionTable(IEnumerable<TypeResolutionEntry> entries)
            : this(entries.ToArray())
        {
        }

        public TypeResolutionTable(params TypeResolutionEntry[] entries)
        {
            if (entries == null)
                throw new ArgumentNullException("entries");

            this.entries = entries;
        }

        public Type Resolve(string localName, string xmlNamespace)
        {
            if (localName == null)
                throw new ArgumentNullException("localName");
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");

            throw new NotImplementedException();
        }

        public IEnumerable<TypeResolutionEntry> Entries
        {
            get { return this.entries; }
        }
    }
}
