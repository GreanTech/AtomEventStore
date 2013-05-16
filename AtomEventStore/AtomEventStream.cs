using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Represents a stream of events. Events can be of (potentially) any type,
    /// as long as there's a storage mechanism that can persist and read back
    /// instances of that type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of event represented by the stream.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// The AtomEventStream class stores and reads events using a Linked List
    /// storage approach. For its particulars, it uses Atom for persistence and
    /// linking.
    /// </para>
    /// <para>
    /// The concepts of storing events as a linked list was inspired by an
    /// article by Yves Reynhout called "Your EventStream is a linked list" at
    /// http://bit.ly/AqearV.
    /// </para>
    /// <para>
    /// When you store an event with <see cref="AppendAsync" /> or
    /// <see cref="OnNext" />, AtomEventStream creates a new Atom entry with
    /// that event. It uses a "previous" link to point to the previous Atom
    /// entry, and it also updates an Atom feed, which contains the index of
    /// the event stream.
    /// </para>
    /// <para>
    /// When you read the event stream, the AtomEventStream starts at the index
    /// and works its way back, yielding events as it goes along. Thus, newest
    /// events are served first, until you stop enumerating, or until you reach
    /// the first event.
    /// </para>
    /// <para>
    /// Various storage mechanisms can be plugged into AtomEventStream, such as
    /// a file-based storage mechanism, or in-memory storage. Third-party
    /// storage add-ins for e.g. cloud-based storage is also an option. A
    /// custom storage mechanism must implement the
    /// <see cref="IAtomEventStorage" /> interface.
    /// </para>
    /// </remarks>
    /// <seealso cref="AtomEventsInMemory" />
    /// <seealso cref="AtomEventsInFiles" />
    /// <seealso cref="IAtomEventStorage" />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Suppressed on common vote by Mark Seemann and Mikkel Christensen, 2013-05-09. See also http://bit.ly/13ioVAG")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suppressed following discussion at http://bit.ly/11T4eZe")]
    public class AtomEventStream<T> : IEnumerable<T>, IObserver<T>
    {
        private readonly UuidIri id;
        private readonly IAtomEventStorage storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEventStream{T}" />
        /// class.
        /// </summary>
        /// <param name="id">The ID of the event stream.</param>
        /// <param name="storage">
        /// The underlying storage mechanism to use.
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
        /// <seealso cref="AtomEventStream{T}" />
        /// <seealso cref="AtomEventsInMemory" />
        /// <seealso cref="AtomEventsInFiles" />
        /// <seealso cref="IAtomEventStorage" />
        public AtomEventStream(
            UuidIri id,
            IAtomEventStorage storage)
        {
            this.id = id;
            this.storage = storage;
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
        /// <em>after</em> all previous events. However, keep in mind that
        /// since <see cref="AtomEventStream{T}" /> iterates over the event
        /// stream from newest to oldest event, the newly appended event will
        /// also be the first item to be enumerated, because it's the most
        /// recent event.
        /// </para>
        /// <para>
        /// Since this method conceptually involves writing the event to the
        /// underlying <see cref="Storage" />, it may take significant time to
        /// complete; for that reason, it's an asynchronous method, returning a
        /// <see cref="Task" />. The operation is not guaranteed to be complete
        /// before the Task completes successfully.
        /// </para>
        /// <para>
        /// When updating the underlying Storage, the method first writes a new
        /// Atom entry representing the serialized event, using
        /// <see cref="IAtomEventStorage.CreateFeedWriterFor(AtomFeed)" />.
        /// Subsequently, it updates the index of the event stream, using
        /// <see cref="IAtomEventStorage.CreateEntryWriterFor(AtomEntry)" />.
        /// Since these two operations are not guaranteed to happen with an
        /// ACID transaction, it's possible that the Atom entry is saved, but
        /// that the update of the index fails. If the underlying storage
        /// throws an exception at that point, that exception will bubble up to
        /// the caller of the AppendAsync method. It's up to the caller to
        /// retry the operation.
        /// </para>
        /// <para>
        /// However, in that situation, an orphaned Atom entry is likely to
        /// have been left in the underlying storage. This doesn't affect
        /// consistency of the system, but may take up unnecessary disk space.
        /// If this is the case, a separate clean-up task should find and
        /// delete orphaned entries.
        /// </para>
        /// </remarks>
        public Task AppendAsync(T @event)
        {
            return Task.Factory.StartNew(() =>
            {
                var now = DateTimeOffset.Now;

                var index = this.ReadIndex();

                var changesetId = Guid.NewGuid();
                var entry = new AtomEntry(
                    changesetId,
                    "Changeset " + changesetId,
                    now,
                    now,
                    new AtomAuthor("Grean"),
                    new XmlAtomContent(@event),
                    CreateLinksForNewEntry(index, changesetId));

                var feed = new AtomFeed(
                    this.id,
                    "Index of event stream " + (Guid)this.id,
                    now,
                    new AtomAuthor("Grean"),
                    new[] { entry.WithLinks(entry.Links.Select(ChangeRelFromSelfToVia)) },
                    new[] { CreateSelfLinkFrom(this.id) });

                using (var w = this.storage.CreateFeedWriterFor(feed))
                    feed.WriteTo(w);
                using (var w = this.storage.CreateEntryWriterFor(entry))
                    entry.WriteTo(w);
            });
        }

        private AtomFeed ReadIndex()
        {
            var indexAddress =
                new Uri(((Guid)this.id).ToString(), UriKind.Relative);
            using (var r = this.storage.CreateFeedReaderFor(indexAddress))
                return AtomFeed.ReadFrom(r);
        }

        private static IEnumerable<AtomLink> CreateLinksForNewEntry(
            AtomFeed index,
            Guid changesetId)
        {
            return (from e in index.Entries
                    from l in e.Links
                    where l.IsViaLink
                    select l.WithRel("previous"))
                    .Take(1)
                    .Concat(new[] { CreateSelfLinkFrom(changesetId) });
        }

        private static AtomLink ChangeRelFromSelfToVia(AtomLink link)
        {
            return link.IsSelfLink ? link.ToViaLink() : link;
        }

        /// <summary>
        /// Gets the id of the event stream.
        /// </summary>
        /// <value>
        /// The id of the event stream, as originally supplied via the
        /// constructor.
        /// </value>
        /// <seealso cref="AtomEventStream{T}(UuidIri, IAtomEventStorage)" />
        public UuidIri Id
        {
            get { return this.id; }
        }

        public IAtomEventStorage Storage
        {
            get { return this.storage; }
        }

        private static AtomLink CreateSelfLinkFrom(Guid id)
        {
            return AtomEventStream.CreateSelfLinkFrom(id);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var entry = this.ReadIndex().Entries.SingleOrDefault();
            while (entry != null)
            {
                yield return Cast(entry.Content.Item);

                entry = this.GetPrevious(entry);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private static T Cast(object item)
        {
            if (item is T)
                return (T)item;

            return (T)TypeDescriptor.GetConverter(item).ConvertTo(item, typeof(T));
        }

        private AtomEntry GetPrevious(AtomEntry current)
        {
            var previousLink = current.Links.SingleOrDefault(
                l => l.Rel == "previous");
            if (previousLink == null)
                return null;

            using (var r = this.storage.CreateEntryReaderFor(previousLink.Href))
                return AtomEntry.ReadFrom(r);
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Suppressed on common vote by Mark Seemann and Mikkel Christensen, 2013-05-09. See also http://bit.ly/13ioVAG")]
    public static class AtomEventStream
    {
        public static AtomLink CreateSelfLinkFrom(Guid id)
        {
            return AtomLink.CreateSelfLink(
                new Uri(id.ToString(), UriKind.Relative));
        }
    }
}
