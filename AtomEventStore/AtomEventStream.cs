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

                var changesetId = UuidIri.NewId();
                var entry = new AtomEntry(
                    changesetId,
                    "Changeset " + (Guid)changesetId,
                    now,
                    now,
                    new AtomAuthor("Grean"),
                    new XmlAtomContent(@event),
                    new[] { CreateSelfLinkFrom(changesetId) });

                var feed = new AtomFeed(
                    this.id,
                    "Head of event stream " + (Guid)this.id,
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

        private static AtomLink ChangeRelFromSelfToVia(AtomLink link)
        {
            if (link.IsSelfLink)
                return link.WithRel("via");
            return link;
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
            return AtomLink.CreateSelfLink(
                new Uri(
                    ((Guid)id).ToString(),
                    UriKind.Relative));
        }
    }
}
