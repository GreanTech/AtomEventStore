using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class AtomEventObserver<T>
    {
        private readonly UuidIri id;
        private readonly IAtomEventStorage storage;
        private readonly IContentSerializer serializer;

        public AtomEventObserver(
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

        public Task AppendAsync(T @event)
        {
            return Task.Factory.StartNew(() =>
            {
                var now = DateTimeOffset.Now;

                var firstId = UuidIri.NewId();

                var index = new AtomFeed(
                    this.id,
                    "Index of ",
                    now,
                    new AtomAuthor("Grean"),
                    Enumerable.Empty<AtomEntry>(),
                    new[]
                    {
                        AtomLink.CreateSelfLink(
                            new Uri(
                                ((Guid)this.id).ToString(),
                                UriKind.Relative)),
                        AtomLink.CreateFirstLink(
                            new Uri(
                                ((Guid)firstId).ToString(),
                                UriKind.Relative))
                    });

                var entry = CreateEntry(@event, now);

                var first = new AtomFeed(
                    firstId,
                    "Partial event stream",
                    now,
                    new AtomAuthor("Grean"),
                    new[] { entry },
                    new[]
                    {
                        AtomLink.CreateSelfLink(
                            new Uri(
                                ((Guid)firstId).ToString(),
                                UriKind.Relative))
                    });

                using (var w = this.storage.CreateFeedWriterFor(index))
                    index.WriteTo(w, this.serializer);

                using (var w = this.storage.CreateFeedWriterFor(first))
                    first.WriteTo(w, this.serializer);
            });
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
