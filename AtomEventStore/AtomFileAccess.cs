using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class AtomFileAccess :
        ISyndicationItemWriter,
        ISyndicationFeedWriter,
        ISyndicationItemReader,
        ISyndicationFeedReader
    {
        private readonly DirectoryInfo directory;

        public AtomFileAccess(DirectoryInfo directory)
        {
            this.directory = directory;
        }

        public SyndicationFeed ReadFeed(string id)
        {
            var fileName = this.CreateFileName(id);
            
            if (File.Exists(fileName))
                using (var r = XmlReader.Create(fileName))
                    return SyndicationFeed.Load(r);

            return new SyndicationFeed();
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

        public SyndicationItem ReadItem(string id)
        {
            var fileName = this.CreateFileName(id);
            using (var r = XmlReader.Create(fileName))
                return SyndicationItem.Load(r);
        }

        public DirectoryInfo Directory
        {
            get { return this.directory; }
        }

        private string CreateFileName(IEnumerable<SyndicationLink> links)
        {
            var selfLink = links.Single(l => l.RelationshipType == "self");
            return this.CreateFileName(selfLink.Uri.ToString());
        }

        private string CreateFileName(string id)
        {
            return Path.Combine(this.directory.ToString(), id);
        }
    }
}
