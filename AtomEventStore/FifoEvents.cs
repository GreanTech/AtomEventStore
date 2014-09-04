using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// A forward-moving sequence of events, read from an underlying storage
    /// mechansism. Events can be of (potentially) any type, as long as there's
    /// a storage mechanism that can persist and read back instances of that
    /// type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of event represented by the stream.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// The FifoEvents class reads events using a Linked List storage approach.
    /// For its particulars, it uses Atom for persistence and linking.
    /// </para>
    /// <para>
    /// The concepts of storing events as a linked list was inspired by an
    /// article by Yves Reynhout called "Your EventStream is a linked list" at
    /// http://bit.ly/AqearV.
    /// </para>
    /// <para>
    /// When you read the event stream, FifoEvents starts at the beginning and
    /// works its way forward, yielding events as it goes along. Thus, oldest
    /// events are served first, until you stop enumerating, or until you reach
    /// the most recent event.
    /// </para>
    /// <para>
    /// Various storage mechanisms can be plugged into FifoEvents, such as a
    /// file-based storage mechanism, or in-memory storage. Third-party storage
    /// add-ins for e.g. cloud-based storage is also an option. A custom
    /// storage mechanism must implement the <see cref="IAtomEventStorage" />
    /// interface.
    /// </para>
    /// <para>
    /// Use <see cref="AtomEventObserver{T}" /> to write the events.
    /// </para>
    /// </remarks>
    /// <seealso cref="AtomEventsInMemory" />
    /// <seealso cref="AtomEventsInFiles" />
    /// <seealso cref="IAtomEventStorage" />
    /// <seealso cref="AtomEventObserver{T}" />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suppressed following discussion at http://bit.ly/11T4eZe")]
    public class FifoEvents<T> : IEnumerable<T>
    {
        private readonly UuidIri id;
        private readonly IAtomEventStorage storage;
        private readonly IContentSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FifoEvents{T}" />
        /// class.
        /// </summary>
        /// <param name="id">The ID of the event stream.</param>
        /// <param name="storage">
        /// The underlying storage mechanism from which to read.
        /// </param>
        /// <param name="serializer">
        /// The serializer used to serialize and deserialize items to a format
        /// compatible with Atom. The object supplied via this constructor
        /// parameter is subsequently available via the
        /// <see cref="Serializer" /> property.
        /// </param>
        /// <remarks>
        /// <para>
        /// The <paramref name="id" /> is the ID of a single event stream. Each
        /// event stream has its own ID. If you need more than a single event
        /// stream (e.g. if you are implementing the Aggregate Root pattern),
        /// each event stream should have a separate ID.
        /// </para>
        /// <para>
        /// The <paramref name="storage" /> value can be any implementation of
        /// <see cref="IAtomEventStorage" />. Built-in implementatoins include
        /// <see cref="AtomEventsInMemory" /> and
        /// <see cref="AtomEventsInFiles" />.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="storage" /> or <paramref name="serializer" /> is
        /// <see langword="null" />
        /// </exception>
        /// <seealso cref="FifoEvents{T}" />
        /// <seealso cref="ContentSerializer" />
        /// <seealso cref="AtomEventsInMemory" />
        /// <seealso cref="AtomEventsInFiles" />
        /// <seealso cref="IAtomEventStorage" />
        public FifoEvents(
            UuidIri id,
            IAtomEventStorage storage,
            IContentSerializer serializer)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            this.id = id;
            this.storage = storage;
            this.serializer = serializer;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the events, from
        /// earliest to the most recent.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator{T}" /> that
        /// can be used to iterate through the collection.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This Iterator reads events from the instance's underlying
        /// <see cref="Storage" />, which may involve I/O operations. Since
        /// AtomEventStore stores events in Atom Feed pages, unless the page
        /// size used is 1, events will tend to be enumerated in bursts
        /// corresponding to the page size.
        /// </para>
        /// <para>
        /// In order to minimize the potential delay caused by I/O operations,
        /// this Iterator employs a read-ahead algorithm. When enumerating
        /// events, it begins to download the next Atom feed page on a
        /// background thread, while still yielding entries from the current
        /// page. This means that the Iterator may occasionally download a page
        /// that the client doesn't need, because the client breaks out of the
        /// enumeration before reaching the page in question.
        /// </para>
        /// <para>
        /// In order to prevent unnecessary page downloads, the algorithm only
        /// starts reading ahead after enumerating the first item in the
        /// sequence. This enables clients to peek at the first item without
        /// triggering a background download.
        /// </para>
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            var page = this.ReadFirst();
            while (page != null)
            {
                var entries = page.Entries.Reverse().ToArray();
                if (!entries.Any())
                    yield break;

                yield return (T)entries.First().Content.Item;

                var t = Task.Factory.StartNew(() => this.ReadNext(page));

                foreach (var entry in entries.Skip(1))
                    yield return (T)entry.Content.Item;

                page = t.Result;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can
        /// be used to iterate through the collection.
        /// </returns>
        /// <seealso cref="GetEnumerator()" />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private AtomFeed ReadFirst()
        {
            var index = this.ReadIndex();
            var firstLink = index.Links.SingleOrDefault(l => l.IsFirstLink);
            if (firstLink == null)
                return null;

            return this.ReadPage(firstLink.Href);
        }

        private AtomFeed ReadNext(AtomFeed page)
        {
            var nextLink = page.Links.SingleOrDefault(l => l.IsNextLink);
            if (nextLink == null)
                return null;

            return this.ReadPage(nextLink.Href);
        }

        private AtomFeed ReadIndex()
        {
            var indexAddress =
                new Uri(
                    ((Guid)this.id) + "/" + ((Guid)this.id),
                    UriKind.Relative);
            return this.ReadPage(indexAddress);
        }

        private AtomFeed ReadPage(Uri address)
        {
            using (var r = this.storage.CreateFeedReaderFor(address))
                return AtomFeed.ReadFrom(r, this.serializer);
        }

        public UuidIri Id
        {
            get { return this.id; }
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
