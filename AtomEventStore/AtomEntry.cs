using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomEntry
    {
        private readonly UuidIri id;

        public AtomEntry(UuidIri id)
        {
            this.id = id;
        }
        
        public UuidIri Id
        {
            get { return this.id; }
        }
    }
}
