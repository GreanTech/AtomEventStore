using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class FifoEvents<T> : IEnumerable<T>
    {
        private readonly UuidIri id;
        private readonly IAtomEventStorage storage;
        private readonly IContentSerializer serializer;

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

        public IEnumerator<T> GetEnumerator()
        {
            var page = this.ReadFirst();
            while (page != null)
            {
                var t = Task.Factory.StartNew(() => this.ReadNext(page));

                foreach (var entry in page.Entries.Reverse())
                    yield return (T)entry.Content.Item;

                page = t.Result;
            }
        }

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
                new Uri(((Guid)this.id).ToString(), UriKind.Relative);
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
