﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomFileWriter : ISyndicationItemWriter, ISyndicationFeedWriter
    {
        private readonly DirectoryInfo directory;

        public AtomFileWriter(DirectoryInfo directory)
        {
            this.directory = directory;
        }

        public void Create(SyndicationItem item)
        {
        }

        public void CreateOrUpdate(SyndicationFeed feed)
        {
        }

        public DirectoryInfo Directory
        {
            get { return this.directory; }
        }
    }
}
