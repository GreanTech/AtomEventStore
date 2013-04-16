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
        private readonly IEnumerable<AtomLink> links;

        public AtomFeed(
            UuidIri id,
            string title, 
            DateTimeOffset updated,
            AtomAuthor author,
            IEnumerable<AtomEntry> entries,
            IEnumerable<AtomLink> links)
        {
            this.id = id;
            this.title = title;
            this.updated = updated;
            this.author = author;
            this.entries = entries;
            this.links = links;
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

        public IEnumerable<AtomLink> Links
        {
            get { return this.links; }
        }

        public AtomFeed WithTitle(string newTitle)
        {
            return new AtomFeed(
                this.id,
                newTitle,
                this.updated,
                this.author,
                this.entries,
                this.links);
        }

        public AtomFeed WithUpdated(DateTimeOffset newUpdated)
        {
            return new AtomFeed(
                this.id,
                this.title,
                newUpdated,
                this.author,
                this.entries,
                this.links);
        }

        public AtomFeed WithAuthor(AtomAuthor newAuthor)
        {
            return new AtomFeed(
                this.id,
                this.title,
                this.updated,
                newAuthor,
                this.entries,
                this.links);
        }
    }
}
