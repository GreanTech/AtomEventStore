using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomEntry
    {
        private readonly UuidIri id;
        private readonly string title;
        private readonly DateTimeOffset published;

        public AtomEntry(UuidIri id, string title, DateTimeOffset published)
        {
            this.id = id;
            this.title = title;
            this.published = published;
        }
        
        public UuidIri Id
        {
            get { return this.id; }
        }

        public string Title
        {
            get { return this.title; }
        }

        public DateTimeOffset Published
        {
            get { return this.published; }
        }
    }
}
