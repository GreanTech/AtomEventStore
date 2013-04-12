using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomEntryFileWriter : ISyndicationItemWriter
    {
        private readonly DirectoryInfo directory;

        public AtomEntryFileWriter(DirectoryInfo directory)
        {
            this.directory = directory;
        }

        public void Create(SyndicationItem item)
        {
        }

        public DirectoryInfo Directory
        {
            get { return this.directory; }
        }
    }
}
