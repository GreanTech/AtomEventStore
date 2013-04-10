using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public struct UuidIri
    {
        private readonly Guid id;

        public UuidIri(Guid id)
        {
            this.id = id;
        }

        public override string ToString()
        {
            return "urn:uuid:" + this.id;
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
            if (!candidate.StartsWith("urn:uuid:"))
            {
                uuidIri = default(UuidIri);
                return false;
            }

            Guid parsedId;
            if (!Guid.TryParse(candidate.Substring("urn:uuid:".Length), out parsedId))
            {
                uuidIri = default(UuidIri);
                return false;
            }

            uuidIri = parsedId;
            return true;
        }
    }
}
