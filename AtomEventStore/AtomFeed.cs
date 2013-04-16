using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomFeed
    {
        private readonly UuidIri id;

        public AtomFeed(UuidIri id)
        {
            this.id = id;
        }

        public UuidIri Id
        {
            get { return this.id; }
        }
    }
}
