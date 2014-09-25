using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Resolves one or more <see cref="TypeResolutionEntry" /> to a
    /// <see cref="Type" />.
    /// </summary>
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

            try
            {
                return this.entries
                    .Single(x =>
                        x.LocalName == localName &&
                        x.XmlNamespace == xmlNamespace)
                    .Resolution;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    string.Format(
                        "The provided local name ({0}) and XML namespace ({1}) could not be mapped to a proper type.",
                        localName,
                        xmlNamespace),
                    e);
            }
        }

        public IEnumerable<TypeResolutionEntry> Entries
        {
            get { return this.entries; }
        }
    }
}
