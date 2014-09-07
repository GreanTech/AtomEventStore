using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// A UUID IRI (Universally Unique IDentifier Internationalized Resource
    /// Identifier)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A UUID is a Universally Unique IDentifier; on Windows implemented as
    /// GUIDs, and surfaced in .NET as <see cref="Guid" /> values.
    /// </para>
    /// <para>
    /// An IRI is an Internationalized Resource Identifier, which is a
    /// generalization of an URI - a Uniform Resource Identifier.
    /// </para>
    /// <para>
    /// The UuidIri value type encapsulates a <see cref="Guid" /> and provides
    /// varous formatting and parsing methods to convert UUID IRIs to and from
    /// Guid values.
    /// </para>
    /// <para>
    /// When converted to a <see cref="String" />, a typical UuidIri looks like
    /// this: "urn:uuid:ce11a36a-61af-4f66-8def-09e32bcce824".
    /// </para>
    /// <para>
    /// UuidIri defines implicit conversions to and from Guid, because
    /// conversions are lossless both ways.
    /// </para>
    /// </remarks>
    public struct UuidIri : IEquatable<UuidIri>
    {
        private const string prefix = "urn:uuid:";
        private readonly Guid id;

        /// <summary>
        /// Initializes a new instance of the <see cref="UuidIri"/> struct.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="id" /> is <see cref="Guid.Empty" />.
        /// </exception>
        public UuidIri(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("The empty Guid is not allowed. Please supply an appropriate Guid value.", "id");

            this.id = id;
        }

        /// <summary>
        /// Returns a <see cref="String" /> that representing this instance,
        /// correctly formatted as a UUID IRI.
        /// </summary>
        /// <returns>
        /// A <see cref="String" /> representing this instance, correctly
        /// formatted as a UUID IRI.
        /// </returns>
        /// <example>
        /// This example demonstrates how to format a UuidIri value as a
        /// string:
        /// <code>
        /// var actual = sut.ToString();
        /// </code>
        /// The output is a <see cref="String" /> equivalent to
        /// "urn:uuid:ce11a36a-61af-4f66-8def-09e32bcce824".
        /// </example>
        public override string ToString()
        {
            return prefix + this.id;
        }

        public static implicit operator Guid(UuidIri uuidIri)
        {
            return uuidIri.id;
        }

        public static implicit operator UuidIri(Guid value)
        {
            return new UuidIri(value);
        }

        public static bool TryParse(string candidate, out UuidIri uuidIri)
        {
            Guid parsedId;
            if (candidate != null &&
                candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(candidate.Substring(prefix.Length), out parsedId))
            {
                uuidIri = parsedId;
                return true;
            }

            uuidIri = default(UuidIri);
            return false;
        }

        public static UuidIri Parse(string candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            UuidIri parsedUuid;
            var couldParse = UuidIri.TryParse(candidate, out parsedUuid);
            if (!couldParse)
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "The candidate string is not a correctly formatted UUID IRI. The valid format is: \"urn:uuid:<Guid value>\". Example: \"urn:uuid:7eddb2e3-369b-4f3d-8697-520bc5de8ed4\". The actual candidate string was: \"{0}\"",
                        candidate),
                    "candidate");
            return parsedUuid;
        }

        public static UuidIri NewId()
        {
            return new UuidIri(Guid.NewGuid());
        }

        public override bool Equals(object obj)
        {
            if (obj is UuidIri)
                return this.Equals((UuidIri)obj);

            return base.Equals(obj);
        }

        public bool Equals(UuidIri other)
        {
            return object.Equals(this.id, other.id);
        }

        public static bool operator ==(UuidIri value1, UuidIri value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(UuidIri value1, UuidIri value2)
        {
            return !value1.Equals(value2);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }
    }
}
