using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Writes events to a specific event stream.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the events.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// <typeparamref name="T"/> can be a polymorphic type such as an interface
    /// or an F# Discriminated Union, as long as the supplied
    /// <see cref="Serializer" /> can round-trip the type (that is: write all
    /// subtypes to XML, and correctly read them from XML and dehydrate them to
    /// their original subtype). Sometimes, an <see cref="ITypeResolver" /> can
    /// be helpful when deserializing.
    /// </para>
    /// <para>
    /// In order to read the event stream, you can use
    /// <see cref="FifoEvents{T}" />.
    /// </para>
    /// <para>
    /// The AtomEventObserver class stores events using a Linked List storage
    /// approach. For its particulars, it uses Atom for persistence and
    /// linking.
    /// </para>
    /// <para>
    /// The concepts of storing events as a linked list was inspired by an
    /// article by Yves Reynhout called "Your EventStream is a linked list" at
    /// http://bit.ly/AqearV.
    /// </para>
    /// <para>
    /// When you store an event with <see cref="AppendAsync" /> or
    /// <see cref="OnNext" />, AtomEventObserver creates a new Atom entry with
    /// that event, and adds it to an Atom feed page. When the number of
    /// entries exceeds the configured <see cref="PageSize" />, a new feed page
    /// is created for the surplus entry, and a "previous" link is added to the
    /// new page, pointing to the previous page, thus establishing a Linked
    /// List of Atom feed pages.
    /// </para>
    /// <para>
    /// Various storage mechanisms can be plugged into AtomEventObserver, such
    /// as a file-based storage mechanism, or in-memory storage. Third-party
    /// storage add-ins for e.g. cloud-based storage is also an option. A
    /// custom storage mechanism must implement the
    /// <see cref="IAtomEventStorage" /> interface, and can optionally benefit
    /// from also implementing <see cref="IEnumerable{UuidIri}" />.
    /// </para>
    /// </remarks>
    /// <seealso cref="AtomEventsInMemory" />
    /// <seealso cref="AtomEventsInFiles" />
    /// <seealso cref="IAtomEventStorage" />
    /// <seealso cref="FifoEvents{T}" />
    public class AtomEventObserver<T> : IObserver<T>
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
                    .DefaultIfEmpty(AtomLink.CreateFirstLink(this.CreateNewFeedAddress()))
                    .Single();
                var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
                var lastLinkChanged = false;
                if (lastLink == null)
                {
                    lastLink = firstLink.ToLastLink();
                    lastLinkChanged = true;
                }
                var lastPage = this.ReadLastPage(lastLink.Href);
                if (lastPage.Links.Single(l => l.IsSelfLink).Href != lastLink.Href)
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
                    var nextAddress = this.CreateNewFeedAddress();
                    var nextPage = this.ReadPage(nextAddress);
                    nextPage = AddEntryTo(nextPage, entry, now);

                    var nextLink = AtomLink.CreateNextLink(nextAddress);

                    var previousPage = lastPage
                        .WithLinks(lastPage.Links.Concat(new[] { nextLink }));

                    var previousLink = previousPage.Links
                        .Single(l => l.IsSelfLink)
                        .ToPreviousLink();

                    nextPage = nextPage.WithLinks(
                        nextPage.Links.Concat(new[] { previousLink }));
                    index = index.WithLinks(index.Links
                        .Where(l => !l.IsLastLink)
                        .Concat(new[] { nextLink.ToLastLink() }));

                    this.Write(nextPage);
                    this.Write(previousPage);
                    this.Write(index);
                }
                else
                {
                    lastPage = AddEntryTo(lastPage, entry, now);

                    this.Write(lastPage);
                    if (lastLinkChanged)
                        this.Write(index);
                }
            });
        }

        private Uri CreateNewFeedAddress()
        {
            var indexedAddress = ((Guid)this.id) + "/" + Guid.NewGuid();
            return new Uri(indexedAddress, UriKind.Relative);
        }

        private AtomFeed ReadIndex()
        {
            var indexAddress =
                new Uri(
                    ((Guid)this.id) + "/" + ((Guid)this.id),
                    UriKind.Relative);
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
            AtomFeed page,
            AtomEntry entry,
            DateTimeOffset now)
        {
            var entries = new[] { entry }.Concat(page.Entries);
            return CreateNewPage(entries, page.Links, now);
        }

        private static AtomFeed CreateNewPage(
            IEnumerable<AtomEntry> entries,
            IEnumerable<AtomLink> links,
            DateTimeOffset now)
        {
            var selfLink = links.Single(l => l.IsSelfLink);
            var id = AtomEventStorage.GetIdFromHref(selfLink.Href);

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

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            this.AppendAsync(value).Wait();
        }
    }
}
