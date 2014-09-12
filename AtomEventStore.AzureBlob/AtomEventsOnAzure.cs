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
    /// <summary>
    /// Stores events in Atom Feeds as BLOBs in Microsoft Azure.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suppressed following discussion at http://bit.ly/11T4eZe")]
    public class AtomEventsOnAzure : IAtomEventStorage, IEnumerable<UuidIri>
    {
        private readonly CloudBlobContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEventsOnAzure" />
        /// class.
        /// </summary>
        /// <param name="container">
        /// The BLOB container where all BLOBs written by this instance will
        /// reside.
        /// </param>
        /// <remarks>
        /// <para>
        /// A single instance of <see cref="AtomEventsOnAzure" /> can manage
        /// several event streams in a single CloudBlobContainer. Several
        /// instances of AtomEventsOnAzure can also read from, and write to,
        /// the same BLOB container.
        /// </para>
        /// <para>
        /// If you need to distribute event streams across different BLOB
        /// container, you can create an AtomEventsOnAzure instance per
        /// container.
        /// </para>
        /// </remarks>
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

        public IEnumerator<UuidIri> GetEnumerator()
        {
            return this.container
                .ListBlobs()
                .OfType<CloudBlobDirectory>()
                .Select(d => d.Uri.Segments.Last())
                .Select(s => s.Trim('/'))
                .Select(TryParseGuid)
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
