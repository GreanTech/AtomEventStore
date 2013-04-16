using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomLink
    {
        private readonly string rel;
        private readonly Uri href;

        public AtomLink(string rel, Uri href)
        {
            this.rel = rel;
            this.href = href;
        }

        public string Rel
        {
            get { return this.rel; }
        }

        public Uri Href
        {
            get { return this.href; }
        }
    }
}
