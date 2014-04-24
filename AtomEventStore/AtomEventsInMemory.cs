using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class AtomEventsInMemory : IAtomEventStorage, IEnumerable<UuidIri>, IDisposable
    {
        private readonly ReaderWriterLockSlim rwLock;
        private readonly Dictionary<Uri, StringBuilder> feeds;
        private readonly List<UuidIri> indexes;

        public AtomEventsInMemory()
        {
            this.rwLock = new ReaderWriterLockSlim();
            this.feeds = new Dictionary<Uri, StringBuilder>();
            this.indexes = new List<UuidIri>();
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            if (atomFeed == null)
                throw new ArgumentNullException("atomFeed");

            var id = GetHrefFrom(atomFeed.Links);
            var sb = new StringBuilder();
            return new CallBackXmlWriter(
                new StringWriter(sb),
                () =>
                {
                    this.rwLock.EnterWriteLock();
                    try
                    {
                        this.AddToIndexesIfIndex(atomFeed);
                        this.feeds[id] = sb;
                    }
                    finally
                    {
                        this.rwLock.ExitWriteLock();
                    }
                });
        }

        private static Uri GetHrefFrom(IEnumerable<AtomLink> links)
        {
            var selfLink = links.Single(l => l.IsSelfLink);
            return selfLink.Href;
        }

        private void AddToIndexesIfIndex(AtomFeed atomFeed)
        {
            /* Look for self links which indicate that this Atom Feed is an
             * indexed index. The pattern to look for is:
             * id/id
             * i.e. a segmented URL where the first and last segment are
             * identical. */
            var selfLink = atomFeed.Links.Single(l => l.IsSelfLink);
            var segments = AtomEventStorage
                .GetSegmentsFrom(selfLink.Href)
                .Select(s => s.Trim('/'))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
            if (segments.Length == 2 && segments[0] == segments[1])
                this.indexes.Add(atomFeed.Id);
        }

        private class CallBackXmlWriter : XmlTextWriter
        {
            private readonly Action callback;

            public CallBackXmlWriter(TextWriter w, Action callback)
                : base(w)
            {
                this.callback = callback;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    this.callback();

                base.Dispose(disposing);
            }
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            if (href == null)
                throw new ArgumentNullException("href");

            this.rwLock.EnterReadLock();
            try
            {
                if (this.feeds.ContainsKey(href))
                    return CreateReaderOver(this.feeds[href].ToString());
                else
                    return AtomEventStorage.CreateNewFeed(href);
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        private static XmlReader CreateReaderOver(string xml)
        {
            var sr = new StringReader(xml);
            try
            {
                return XmlReader.Create(
                    sr,
                    new XmlReaderSettings { CloseInput = true });
            }
            catch
            {
                sr.Dispose();
                throw;
            }
        }

        public IEnumerable<string> Feeds
        {
            get
            {
                this.rwLock.EnterReadLock();
                try
                {
                    return this.feeds.Values.Select(sb => sb.ToString());
                }
                finally
                {
                    this.rwLock.ExitReadLock();
                }
            }
        }

        public IEnumerator<UuidIri> GetEnumerator()
        {
            this.rwLock.EnterReadLock();
            try
            {
                return this.indexes.GetEnumerator();
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.rwLock.Dispose();
        }
    }
}
