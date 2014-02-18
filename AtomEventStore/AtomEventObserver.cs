﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class AtomEventObserver<T>
    {
        private readonly UuidIri id;
        private readonly int pageSize;
        private readonly IAtomEventStorage storage;
        private readonly IContentSerializer serializer;

        public AtomEventObserver(
            UuidIri id,
            int pageSize,
            IAtomEventStorage storage,
            IContentSerializer serializer)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            this.id = id;
            this.pageSize = pageSize;
            this.storage = storage;
            this.serializer = serializer;
        }

        public Task AppendAsync(T @event)
        {
            return Task.Factory.StartNew(() =>
            {
                var now = DateTimeOffset.Now;

                var index = this.ReadIndex();
                var firstLink = index.Links
                    .Where(l => l.IsFirstLink)
                    .DefaultIfEmpty(AtomLink.CreateFirstLink(new Uri(Guid.NewGuid().ToString(), UriKind.Relative)))
                    .Single();
                var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
                var lastLinkChanged = false;
                if (lastLink == null)
                {
                    lastLink = firstLink.ToLastLink();
                    lastLinkChanged = true;
                }
                var lastPage = this.ReadLastPage(lastLink.Href);
                if(lastPage.Links.Single(l => l.IsSelfLink).Href != lastLink.Href)
                {
                    lastLink = lastPage.Links.Single(l => l.IsSelfLink).ToLastLink();
                    lastLinkChanged = true;
                }
                index = index.WithLinks(index.Links.Union(new[] { firstLink }));
                index = index.WithLinks(index.Links
                    .Where(l => !l.IsLastLink)
                    .Concat(new[] { lastLink }));

                var entry = CreateEntry(@event, now);

                if (lastPage.Entries.Count() >= this.pageSize)
                {
                    var nextId = UuidIri.NewId();
                    var nextAddress = new Uri(((Guid)nextId).ToString(), UriKind.Relative);
                    var nextPage = this.ReadPage(nextAddress);
                    nextPage = AddEntryTo(nextId, nextPage, entry, now);

                    var nextLink = AtomLink.CreateNextLink(nextAddress);
                    var previousPage = lastPage;
                    if (!lastPage.Links.Any(l => l.IsNextLink))
                        previousPage = lastPage
                            .WithLinks(lastPage.Links.Concat(new[] { nextLink }));
                    index = index.WithLinks(index.Links
                        .Where(l => !l.IsLastLink)
                        .Concat(new[] { nextLink.ToLastLink() }));

                    this.Write(nextPage);
                    this.Write(previousPage);
                    this.Write(index);
                }
                else
                {
                    UuidIri lastId = Guid.Parse(lastLink.Href.ToString());
                    lastPage = AddEntryTo(lastId, lastPage, entry, now);

                    this.Write(lastPage);
                    if (lastLinkChanged)
                        this.Write(index);
                }
            });
        }

        private AtomFeed ReadIndex()
        {
            var indexAddress =
                new Uri(((Guid)this.id).ToString(), UriKind.Relative);
            return ReadPage(indexAddress);
        }

        private AtomFeed ReadPage(Uri address)
        {
            using (var r = this.storage.CreateFeedReaderFor(address))
                return AtomFeed.ReadFrom(r, this.serializer);
        }

        private AtomFeed ReadLastPage(Uri address)
        {
            var page = this.ReadPage(address);
            var nextLink = page.Links.SingleOrDefault(l => l.IsNextLink);
            while (nextLink != null)
            {
                page = this.ReadPage(nextLink.Href);
                nextLink = page.Links.SingleOrDefault(l => l.IsNextLink);
            }

            return page;
        }

        private static AtomEntry CreateEntry(T @event, DateTimeOffset now)
        {
            var changesetId = Guid.NewGuid();
            return new AtomEntry(
                changesetId,
                "Changeset " + changesetId,
                now,
                now,
                new AtomAuthor("Grean"),
                new XmlAtomContent(@event),
                new AtomLink[0]);
        }

        private static AtomFeed AddEntryTo(
            UuidIri id,
            AtomFeed page,
            AtomEntry entry,
            DateTimeOffset now)
        {
            var entries = new[] { entry }.Concat(page.Entries);
            return CreateNewPage(id, entries, page.Links, now);
        }

        private static AtomFeed CreateNewPage(
            UuidIri id,
            IEnumerable<AtomEntry> entries,
            IEnumerable<AtomLink> links,
            DateTimeOffset now)
        {
            return new AtomFeed(
                id,
                "Partial event stream",
                now,
                new AtomAuthor("Grean"),
                entries,
                links);
        }

        private void Write(AtomFeed feed)
        {
            using (var w = this.storage.CreateFeedWriterFor(feed))
                feed.WriteTo(w, this.serializer);
        }

        public UuidIri Id
        {
            get { return this.id; }
        }

        public int PageSize
        {
            get { return this.pageSize; }
        }

        public IAtomEventStorage Storage
        {
            get { return this.storage; }
        }

        public IContentSerializer Serializer
        {
            get { return this.serializer; }
        }
    }
}
