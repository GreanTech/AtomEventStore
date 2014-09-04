using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Stores events in Atom Feeds as files on disk.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suppressed following discussion at http://bit.ly/11T4eZe")]
    public class AtomEventsInFiles : IAtomEventStorage, IEnumerable<UuidIri>
    {
        private readonly DirectoryInfo directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEventsInFiles"/>
        /// class.
        /// </summary>
        /// <param name="directory">
        /// The base directory that will be used to store the Atom files.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="directory" /> is <see langword="null" />.
        /// </exception>
        public AtomEventsInFiles(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");

            this.directory = directory;
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            if (href == null)
                throw new ArgumentNullException("href");

            var fileName = this.CreateFileName(href);

            if (File.Exists(fileName))
                return XmlReader.Create(fileName);

            return AtomEventStorage.CreateNewFeed(href);
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            if (atomFeed == null)
                throw new ArgumentNullException("atomFeed");

            var fileName = this.CreateFileName(atomFeed.Links);
            var dir = Path.GetDirectoryName(fileName);
            System.IO.Directory.CreateDirectory(dir);
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
            return Path.Combine(
                this.directory.ToString(),
                href.ToString() + ".xml");
        }

        public IEnumerator<UuidIri> GetEnumerator()
        {
            return this.directory
                .EnumerateDirectories()
                .Select(d => TryParseGuid(d.Name))
                .Where(t => t.Item1)
                .Select(t => (UuidIri)t.Item2)
                .GetEnumerator();
        }

        private static Tuple<bool, Guid> TryParseGuid(string candidate)
        {
            Guid id;
            var success = Guid.TryParse(candidate, out id);
            return new Tuple<bool, Guid>(success, id);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
