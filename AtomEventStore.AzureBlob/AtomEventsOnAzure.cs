using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Grean.AtomEventStore.AzureBlob
{
    public class AtomEventsOnAzure : IAtomEventStorage
    {
        private readonly CloudBlobContainer container;

        public AtomEventsOnAzure(CloudBlobContainer container)
        {
            this.container = container;
        }

        public XmlReader CreateEntryReaderFor(Uri href)
        {
            var blobRef = this.CreateBlobReference(href);
            return XmlReader.Create(
                blobRef.OpenRead(),
                new XmlReaderSettings { CloseInput = true });
        }

        public XmlWriter CreateEntryWriterFor(AtomEntry atomEntry)
        {
            if (atomEntry == null)
                throw new ArgumentNullException("atomEntry");

            var blobRef = this.CreateBlobReference(atomEntry.Links);            
            return XmlWriter.Create(
                blobRef.OpenWrite(),
                new XmlWriterSettings { CloseOutput = true });
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            var blobRef = this.CreateBlobReference(href);
            if (blobRef.Exists())
                return XmlReader.Create(
                    blobRef.OpenRead(),
                    new XmlReaderSettings { CloseInput = true });

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

            var blobRef = this.CreateBlobReference(atomFeed.Links);
            return XmlWriter.Create(
                blobRef.OpenWrite(),
                new XmlWriterSettings { CloseOutput = true });
        }

        private CloudBlockBlob CreateBlobReference(IEnumerable<AtomLink> links)
        {
            var selfLink = links.Single(l => l.IsSelfLink);
            return this.CreateBlobReference(selfLink.Href);
        }

        private CloudBlockBlob CreateBlobReference(Uri href)
        {
            return this.container.GetBlockBlobReference(href.ToString());
        }
    }
}
