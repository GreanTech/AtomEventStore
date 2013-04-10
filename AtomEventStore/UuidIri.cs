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
    }
}
