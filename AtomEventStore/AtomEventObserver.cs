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

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEventObserver{T}" /> class.
        /// </summary>
        /// <param name="id">The ID of the event stream.</param>
        /// <param name="pageSize">
        /// The maxkum page size; that is: the maximum number of instances of
        /// T stored in a single Atom feed page.
        /// </param>
        /// <param name="storage">
        /// The underlying storage mechanism to use.
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
        /// <see cref="IAtomEventStorage" />. Built-in implementations include
        /// <see cref="AtomEventsInMemory" /> and
        /// <see cref="AtomEventsInFiles" />.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="storage" /> or <paramref name="serializer" /> is
        /// <see langword="null" />.
        /// </exception>
        /// <seealso cref="AtomEventObserver{T}" />
        /// <seealso cref="Serializer" />
        /// <seealso cref="AtomEventsInMemory" />
        /// <seealso cref="AtomEventsInFiles" />
        /// <seealso cref="IAtomEventStorage" />
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

        /// <summary>
        /// Appends an event to the event stream.
        /// </summary>
        /// <param name="event">
        /// The event to append to the event stream.
        /// </param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation of
        /// appending the event to the event stream.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method appends <paramref name="event" /> to the current event
        /// stream. Appending an event indicates that it happened
        /// <em>after</em> all previous events.
        /// </para>
        /// <para>
        /// Since this method conceptually involves writing the event to the
        /// underlying <see cref="Storage" />, it may take significant time to
        /// complete; for that reason, it's an asynchronous method, returning a
        /// <see cref="Task" />. The operation is not guaranteed to be complete
        /// before the Task completes successfully.
        /// </para>
        /// <para>
        /// When updating the underlying Storage, the method typically only
        /// updates the index feed, using
        /// <see cref="IAtomEventStorage.CreateFeedWriterFor(AtomFeed)" />.
        /// However, when the number of entries in the index surpasses
        /// <see cref="PageSize" />, a new feed page is created for the new
        /// entry. This page is also written using the CreateFeedWriterFor
        /// method, and only after this succeeds is the old feed page updated
        /// with a link to the new page. Since these two operations are not
        /// guaranteed to happen within an ACID transaction, it's possible that
        /// the new page is saved, but that the update of the old page fails.
        /// If the underlying storage throws an exception at that point, that
        /// exception will bubble up to the caller of the AppendAsync method.
        /// It's up to the caller to retry the operation.
        /// </para>
        /// <para>
        /// However, in that situation, an orphaned Atom feed page is likely to
        /// have been left in the underlying storage. This doesn't affect
        /// consistency of the system, but may take up unnecessary disk space.
        /// If this is the case, a separate clean-up task should find and
        /// delete orphaned pages.
        /// </para>
        /// </remarks>
        /// <example>
        /// This example shows how to create a UserCreated event and write it
        /// using the AppendAsync method. Notice that since
        /// AtomEventObserver&lt;T&gt; uses the standard Task Parallel Library
        /// (TPL) model, you can use it with 'async' and 'await'.
        /// <code>
        /// var obs = new AtomEventObserver&lt;IUserEvent&gt;(
        ///     eventStreamId, // a Guid
        ///     pageSize,      // an Int32
        ///     storage,       // an IAtomEventStorage object
        ///     serializer);   // an IContentSerializer object
        ///
        /// var userCreated = new UserCreated
        /// {
        ///     UserId = eventStreamId,
        ///     UserName = "ploeh",
        ///     Password = "12345",
        ///     Email = "ploeh@fnaah.com"
        /// };
        /// await obs.AppendAsync(userCreated);
        /// </code>
        /// </example>
        /// <seealso cref="OnNext" />
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
                index = index.WithLinks(index.Links.Union(new[] { firstLink }));

                var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
                var lastLinkAdded = false;
                if (lastLink == null)
                {
                    lastLink = firstLink.ToLastLink();
                    lastLinkAdded = true;
                }
                var lastPage = this.ReadTrueLastPage(lastLink.Href);
                var lastLinkCorrected = false;
                if (lastPage.Links.Single(l => l.IsSelfLink).Href != lastLink.Href)
                {
                    lastLink = lastPage.Links.Single(l => l.IsSelfLink).ToLastLink();
                    lastLinkCorrected = true;
                }
                index = index.WithLinks(index.Links
                    .Where(l => !l.IsLastLink)
                    .Concat(new[] { lastLink }));

                var ctx = new AppendContext(
                    index,
                    lastPage,
                    now,
                    lastLinkAdded,
                    lastLinkCorrected);

                var entry = CreateEntry(@event, now);

                if (this.PageSizeReached(lastPage))
                    this.WriteEntryToNewPage(entry, index, lastPage, now, ctx);
                else
                    this.WriteEntryToExistingPage(entry, index, lastPage, now, ctx);
            });
        }

        private class AppendContext
        {
            public readonly AtomFeed Index;
            public readonly AtomFeed LastPage;
            public readonly DateTimeOffset Now;
            public readonly bool LastLinkAdded;
            public readonly bool LastLinkCorrected;

            public AppendContext(
                AtomFeed index,
                AtomFeed lastPage,
                DateTimeOffset now,
                bool lastLinkAdded,
                bool lastLinkCorrected)
            {
                this.Index = index;
                this.LastPage = lastPage;
                this.Now = now;
                this.LastLinkAdded = lastLinkAdded;
                this.LastLinkCorrected = lastLinkCorrected;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Since the offending exception handling block wraps around a piece of behaviour that ultimately is implemented behind an interface, there's no way to know what type of exception can be thrown. Since it's important to suppress any exceptions in this special case, all exception types must be suppressed. Frankly, I can't think of a better solution, but I'm open to suggestions.")]
        private void WriteEntryToNewPage(
            AtomEntry entry,
            AtomFeed index,
            AtomFeed lastPage,
            DateTimeOffset now,
            AppendContext context)
        {
            var newAddress = this.CreateNewFeedAddress();
            var newPage = this.ReadPage(newAddress);
            newPage = AddEntryTo(newPage, entry, now);

            var nextLink = AtomLink.CreateNextLink(newAddress);

            var previousPage = lastPage
                .WithLinks(lastPage.Links.Concat(new[] { nextLink }));

            var previousLink = previousPage.Links
                .Single(l => l.IsSelfLink)
                .ToPreviousLink();

            newPage = newPage.WithLinks(
                newPage.Links.Concat(new[] { previousLink }));
            index = index.WithLinks(index.Links
                .Where(l => !l.IsLastLink)
                .Concat(new[] { nextLink.ToLastLink() }));

            this.Write(newPage);
            this.Write(previousPage);
            try { this.Write(index); } catch { }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Since the offending exception handling block wraps around a piece of behaviour that ultimately is implemented behind an interface, there's no way to know what type of exception can be thrown. Since it's important to suppress any exceptions in this special case, all exception types must be suppressed. Frankly, I can't think of a better solution, but I'm open to suggestions.")]
        private void WriteEntryToExistingPage(
            AtomEntry entry,
            AtomFeed index,
            AtomFeed lastPage,
            DateTimeOffset now,
            AppendContext context)
        {
            lastPage = AddEntryTo(lastPage, entry, now);

            this.Write(lastPage);
            if (context.LastLinkAdded)
                this.Write(index);
            else if (context.LastLinkCorrected)
                try { this.Write(index); } catch { }
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

        private AtomFeed ReadTrueLastPage(Uri address)
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
            return page
                .WithEntries(entries)
                .WithUpdated(now);
        }

        private void Write(AtomFeed feed)
        {
            using (var w = this.storage.CreateFeedWriterFor(feed))
                feed.WriteTo(w, this.serializer);
        }

        private bool PageSizeReached(AtomFeed page)
        {
            return page.Entries.Count() >= this.pageSize;
        }

        /// <summary>
        /// Gets the ID of the event stream.
        /// </summary>
        /// <value>
        /// The ID of the event stream, as originally supplied via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomEventObserver{T}(UuidIri, int, IAtomEventStorage, IContentSerializer)" />
        public UuidIri Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the maximum page size.
        /// </summary>
        /// <value>
        /// The maximum page size, measured in numbers of entries per Atom feed
        /// page. This value is supplied via the constructor.
        /// </value>
        /// <seealso cref="AtomEventObserver{T}(UuidIri, int, IAtomEventStorage, IContentSerializer)" />
        /// <seealso cref="AtomEventObserver{T}" />
        /// <seealso cref="AppendAsync(T)" />
        /// <seealso cref="OnNext" />
        public int PageSize
        {
            get { return this.pageSize; }
        }

        /// <summary>
        /// Gets the underlying storage mechanism.
        /// </summary>
        /// <value>
        /// The underlying storage mechanism, as originally supplied via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomEventObserver{T}(UuidIri, int, IAtomEventStorage, IContentSerializer)" />
        public IAtomEventStorage Storage
        {
            get { return this.storage; }
        }

        /// <summary>
        /// Gets the content serializer.
        /// </summary>
        /// <value>
        /// The content serializer, which is used to serialize and deserialize
        /// the elements of the stream to and from the Atom 'content' element
        /// within an Atom entry. This object is supplied via the constructor.
        /// </value>
        /// <remarks>
        /// <para>
        /// The serializer is used to serialize elements in order to persist 
        /// them. This happens when you append items to the stream.
        /// </para>
        /// </remarks>
        /// <seealso cref="AppendAsync(T)" />
        /// <seealso cref="OnNext" />
        /// <seealso cref="AtomEventObserver{T}(UuidIri, int, IAtomEventStorage, IContentSerializer)" />
        public IContentSerializer Serializer
        {
            get { return this.serializer; }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method only exists because it's part of the
        /// <see cref="IObserver{T}" /> interface.
        /// </para>
        /// </remarks>
        public void OnCompleted()
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="error">
        /// An object that provides additional information about the error.
        /// Ignored by this particular implementation.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method only exists because it's part of the
        /// <see cref="IObserver{T}" /> interface.
        /// </para>
        /// </remarks>
        public void OnError(Exception error)
        {
        }

        /// <summary>
        /// Provides the <see cref="AtomEventObserver{T}" /> with a new event,
        /// appending it to the event stream.
        /// </summary>
        /// <param name="value">The event appended to the event stream.</param>
        /// <remarks>
        /// <para>
        /// Invoking this method appends <paramref name="value" /> to the event
        /// stream and blocks until the write and update operations are
        /// completed. Since it uses the underlying <see cref="Storage" />
        /// mechanism to write the event to persistent storage, and since this
        /// may involve I/O, the execution time of this method can be
        /// significant.
        /// </para>
        /// <para>
        /// For an asynchronous alternative, use <see cref="AppendAsync(T)" />.
        /// </para>
        /// </remarks>
        /// <example>
        /// This example shows how to create a UserCreated event and write it
        /// using the OnNext method. It's not necessary to declare the 'obs'
        /// variable using explicit type declation; the 'var' keyword can be
        /// used as well. This example only uses the explicit type declaration
        /// to make it clearer that 'obs' can be treated as an
        /// <see cref="IEnumerable{T}" />.
        /// <code>
        /// IObserver&lt;IUserEvent&gt; obs = new AtomEventObserver&lt;IUserEvent&gt;(
        ///     eventStreamId, // a Guid
        ///     pageSize,      // an Int32
        ///     storage,       // an IAtomEventStorage object
        ///     serializer);   // an IContentSerializer object
        ///
        /// var userCreated = new UserCreated
        /// {
        ///     UserId = eventStreamId,
        ///     UserName = "ploeh",
        ///     Password = "12345",
        ///     Email = "ploeh@fnaah.com"
        /// };
        /// obs.OnNext(userCreated);
        /// </code>
        /// </example>
        /// <seealso cref="AppendAsync(T)" />
        public void OnNext(T value)
        {
            this.AppendAsync(value).Wait();
        }
    }
}
