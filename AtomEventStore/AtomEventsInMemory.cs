using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class AtomEventsInMemory : IAtomEventStorage, IEnumerable<UuidIri>
    {
        private readonly Dictionary<Uri, StringBuilder> feeds;
        private readonly List<UuidIri> indexes;

        public AtomEventsInMemory()
        {
            this.feeds = new Dictionary<Uri, StringBuilder>();
            this.indexes = new List<UuidIri>();
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            if (atomFeed == null)
                throw new ArgumentNullException("atomFeed");

            this.AddToIndexesIfIndex(atomFeed);

            var id = GetHrefFrom(atomFeed.Links);
            var sb = new StringBuilder();
            this.feeds[id] = sb;
            return XmlWriter.Create(sb);
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            if (href == null)
                throw new ArgumentNullException("href");

            if (this.feeds.ContainsKey(href))
                return CreateReaderOver(this.feeds[href].ToString());
            else
                return AtomEventStorage.CreateNewFeed(href);
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
                return this.feeds.Values.Select(sb => sb.ToString());
            }
        }

        public IEnumerator<UuidIri> GetEnumerator()
        {
            return this.indexes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
