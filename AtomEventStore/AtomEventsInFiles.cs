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
            this.directory = directory;
        }

        public XmlReader CreateEntryReaderFor(Uri href)
        {
            throw new NotImplementedException();
        }

        public XmlWriter CreateEntryWriterFor(AtomEntry atomEntry)
        {
            throw new NotImplementedException();
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            throw new NotImplementedException();
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            throw new NotImplementedException();
        }

        public DirectoryInfo Directory
        {
            get { return this.directory; }
        }
    }
}
