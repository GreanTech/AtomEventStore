using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class LifoEvents<T> : IEnumerable<T>
    {
        private readonly UuidIri id;
        private readonly IAtomEventStorage storage;
        private readonly IContentSerializer serializer;

        public LifoEvents(
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

        public IEnumerator<T> GetEnumerator()
        {
            var page = this.ReadLast();
            while (page != null)
            {
                var entries = page.Entries.ToArray();
                if (!entries.Any())
                    yield break;

                yield return (T)entries.First().Content.Item;

                var t = Task.Factory.StartNew(() => this.ReadNext(page));

                foreach (var entry in entries.Skip(1))
                    yield return (T)entry.Content.Item;

                page = t.Result;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private AtomFeed ReadLast()
        {
            var index = this.ReadIndex();
            var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
            if (lastLink == null)
                return null;

            return this.ReadPage(lastLink.Href);
        }

        private AtomFeed ReadNext(AtomFeed page)
        {
            var nextLink = page.Links.SingleOrDefault(l => l.IsPreviousLink);
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
