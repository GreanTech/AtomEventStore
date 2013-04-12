using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class AtomFileAccess : ISyndicationItemWriter, ISyndicationFeedWriter
    {
        private readonly DirectoryInfo directory;

        public AtomFileAccess(DirectoryInfo directory)
        {
            this.directory = directory;
        }

        public void Create(SyndicationItem item)
        {
            var fileName = this.CreateFileName(item.Links);
            using (var w = XmlWriter.Create(fileName))
                item.SaveAsAtom10(w);
        }

        public void CreateOrUpdate(SyndicationFeed feed)
        {
            var fileName = this.CreateFileName(feed.Links);
            using (var w = XmlWriter.Create(fileName))
                feed.SaveAsAtom10(w);
        }

        public DirectoryInfo Directory
        {
            get { return this.directory; }
        }

        private string CreateFileName(IEnumerable<SyndicationLink> links)
        {
            var selfLink = links.Single(l => l.RelationshipType == "self");
            return Path.Combine(
                this.directory.ToString(),
                selfLink.Uri.ToString());
        }
    }
}
