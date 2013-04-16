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
        private readonly DateTimeOffset updated;
        private readonly AtomAuthor author;

        public AtomEntry(
            UuidIri id,
            string title, 
            DateTimeOffset published,
            DateTimeOffset updated,
            AtomAuthor author)
        {
            this.id = id;
            this.title = title;
            this.published = published;
            this.updated = updated;
            this.author = author;
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

        public DateTimeOffset Updated
        {
            get { return this.updated; }
        }

        public AtomAuthor Author
        {
            get { return this.author; }
        }
    }
}
