using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class AtomEventsInFiles : IAtomEventStorage
    {
        private readonly DirectoryInfo directory;

        public AtomEventsInFiles(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");

            this.directory = directory;
        }

        public XmlReader CreateEntryReaderFor(Uri href)
        {
            if (href == null)
                throw new ArgumentNullException("href");

            var fileName = this.CreateFileName(href);
            return XmlReader.Create(fileName);
        }

        public XmlWriter CreateEntryWriterFor(AtomEntry atomEntry)
        {
            if (atomEntry == null)
                throw new ArgumentNullException("atomEntry");

            var fileName = this.CreateFileName(atomEntry.Links);
            return XmlWriter.Create(fileName);
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            if (href == null)
                throw new ArgumentNullException("href");

            var fileName = this.CreateFileName(href);

            if (File.Exists(fileName))
                return XmlReader.Create(fileName);

            var id = new Guid(href.ToString());
            var xml = new AtomFeed(
                id,
                "Index of event stream " + id,
                DateTimeOffset.Now,
                new AtomAuthor("Grean"),
                Enumerable.Empty<AtomEntry>(),
                new[]
                {
                    AtomEventStream.CreateSelfLinkFrom(id)
                })
                .ToXmlString();

            var sr = new StringReader(xml);
            return XmlReader.Create(
                sr,
                new XmlReaderSettings { CloseInput = true });
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            if (atomFeed == null)
                throw new ArgumentNullException("atomFeed");

            var fileName = this.CreateFileName(atomFeed.Links);
            return XmlWriter.Create(fileName);
        }

        public DirectoryInfo Directory
        {
            get { return this.directory; }
        }

        private string CreateFileName(IEnumerable<AtomLink> links)
        {
            var selfLink = links.Single(l => l.IsSelfLink);
            return this.CreateFileName(selfLink.Href);
        }

        private string CreateFileName(Uri href)
        {
            return Path.Combine(this.directory.ToString(), href.ToString());
        }
    }
}
