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

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeResolutionTable"/>
        /// class.
        /// </summary>
        /// <param name="entries">The entries of the resolution table.</param>
        /// <remarks>
        /// <para>
        /// The constructor arguments are subsequently available on the object
        /// as properties.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="entries" /> is <see langword="null" />.
        /// </exception>
        public TypeResolutionTable(
            IReadOnlyCollection<TypeResolutionEntry> entries)
            : this(entries.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeResolutionTable"/>
        /// class.
        /// </summary>
        /// <param name="entries">The entries of the resolution table.</param>
        /// <remarks>
        /// <para>
        /// The constructor arguments are subsequently available on the object
        /// as properties.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="entries" /> is <see langword="null" />.
        /// </exception>
        public TypeResolutionTable(params TypeResolutionEntry[] entries)
        {
            if (entries == null)
                throw new ArgumentNullException("entries");

            this.entries = entries;
        }

        /// <summary>
        /// Resolves one or more <see cref="TypeResolutionEntry" /> to a
        /// <see cref="Type" />.
        /// </summary>
        /// <param name="localName">
        /// The name of an XML element or attribute.
        /// </param>
        /// <param name="xmlNamespace">
        /// The XML namespace in which <paramref name="localName" /> is
        /// defined.
        /// </param>
        /// <returns>
        /// A <see cref="Type" /> corresponding to
        /// <paramref name="localName" /> and <paramref name="xmlNamespace" />.
        /// </returns>
        public Type Resolve(string localName, string xmlNamespace)
        {
            if (localName == null)
                throw new ArgumentNullException("localName");
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");

            var result = this.entries
                .SingleOrDefault(x =>
                    x.LocalName == localName &&
                    x.XmlNamespace == xmlNamespace);

            if (result == null)
                throw new ArgumentException(
                    string.Format(
                        System.Globalization.CultureInfo.CurrentCulture,
                        "The provided local name ({0}) and XML namespace ({1}) could not be mapped to a proper type.",
                        localName,
                        xmlNamespace));
            return result.ResolvedType;
        }

        /// <summary>
        /// Gets the entries of the resolution table.
        /// </summary>
        /// <value>
        /// The entries of the resolution table, as originally provided via the
        /// constructor.
        /// </value>
        /// <seealso cref="TypeResolutionTable(IReadOnlyCollection{TypeResolutionEntry})" />
        public IEnumerable<TypeResolutionEntry> Entries
        {
            get { return this.entries; }
        }
    }
}
