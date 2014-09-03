using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Resolves XML names to .NET types.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        /// Resolves an XML name, consisting of a local name, and a namepace,
        /// to a <see cref="Type" />.
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
        /// <remarks>
        /// <para>
        /// <strong>Note to implementers:</strong>
        /// </para>
        /// <para>
        /// Return a correct <see cref="Type" /> corresponding to
        /// <paramref name="localName" /> and <paramref name="xmlNamespace" />.
        /// If the implementation doesn't know how to map input to a proper
        /// type, it should throw an appropriate exception (e.g.
        /// <see cref="ArgumentException" />). Returning
        /// <see langword="null" /> is considered a bug in the implementation.
        /// </para>
        /// </remarks>
        Type Resolve(string localName, string xmlNamespace);
    }
}
