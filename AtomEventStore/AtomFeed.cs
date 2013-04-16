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
        private readonly AtomAuthor author;
        private readonly IEnumerable<AtomEntry> entries;

        public AtomFeed(
            UuidIri id,
            string title, 
            DateTimeOffset updated,
            AtomAuthor author,
            IEnumerable<AtomEntry> entries)
        {
            this.id = id;
            this.title = title;
            this.updated = updated;
            this.author = author;
            this.entries = entries;
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

        public AtomAuthor Author
        {
            get { return this.author; }
        }

        public IEnumerable<AtomEntry> Entries
        {
            get { return this.entries; }
        }
    }
}
