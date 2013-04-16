using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomLink
    {
        private readonly string rel;

        public AtomLink(string rel)
        {
            this.rel = rel;
        }

        public string Rel
        {
            get { return this.rel; }
        }
    }
}
