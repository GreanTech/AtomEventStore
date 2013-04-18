using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class AtomEventStream<T>
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

                var changesetId = UuidIri.NewId();
                var entry = new AtomEntry(
                    changesetId,
                    "Changeset " + (Guid)changesetId,
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
            UuidIri changesetId)
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

        private static AtomLink CreateSelfLinkFrom(UuidIri id)
        {
            return AtomEventStream.CreateSelfLinkFrom(id);
        }
    }

    public static class AtomEventStream
    {
        public static AtomLink CreateSelfLinkFrom(UuidIri id)
        {
            return AtomLink.CreateSelfLink(
                new Uri(
                    ((Guid)id).ToString(),
                    UriKind.Relative));
        }
    }
}
