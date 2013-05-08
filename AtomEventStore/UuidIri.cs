using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public struct UuidIri
    {
        private const string prefix = "urn:uuid:";
        private readonly Guid id;

        public UuidIri(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("The empty Guid is not allowed. Please supply an appropriate Guid value.", "id");

            this.id = id;
        }

        public override string ToString()
        {
            return prefix + this.id;
        }

        public static implicit operator Guid(UuidIri uuidIri)
        {
            return uuidIri.id;
        }

        public static implicit operator UuidIri(Guid guid)
        {
            return new UuidIri(guid);
        }

        public static bool TryParse(string candidate, out UuidIri uuidIri)
        {
            Guid parsedId;
            if (candidate != null &&
                candidate.StartsWith(prefix) &&
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
    }
}
