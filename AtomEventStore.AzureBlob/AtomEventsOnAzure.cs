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

        [CLSCompliant(false)]
        public AtomEventsOnAzure(CloudBlobContainer container)
        {
            this.container = container;
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            var blobRef = this.CreateBlobReference(href);
            if (blobRef.Exists())
                return XmlReader.Create(
                    blobRef.OpenRead(),
                    new XmlReaderSettings { CloseInput = true });

            return AtomEventStorage.CreateNewFeed(href);
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            if (atomFeed == null)
                throw new ArgumentNullException("atomFeed");

            var blobRef = this.CreateBlobReference(atomFeed.Links);
            blobRef.Properties.ContentType = "application/xml";
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
            return this.container.GetBlockBlobReference(href.ToString() + ".xml");
        }
    }
}
