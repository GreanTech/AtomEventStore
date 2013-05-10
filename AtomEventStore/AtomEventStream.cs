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

        public AtomEventStream(
            UuidIri id,
            IAtomEventStorage storage)
        {
            this.id = id;
            this.storage = storage;
        }

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
