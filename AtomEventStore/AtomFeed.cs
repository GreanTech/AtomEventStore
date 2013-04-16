using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomFeed
    {
        private readonly UuidIri id;
        private readonly string title;
        private readonly DateTimeOffset updated;

        public AtomFeed(UuidIri id, string title, DateTimeOffset updated)
        {
            this.id = id;
            this.title = title;
            this.updated = updated;
        }

        public UuidIri Id
        {
            get { return this.id; }
        }

        public string Title
        {
            get { return this.title; }
        }

        public DateTimeOffset Updated
        {
            get { return this.updated; }
        }
    }
}
