using System;
using System.Collections.Generic;
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

        public static UuidIri NewId()
        {
            return new UuidIri(Guid.NewGuid());
        }
    }
}
