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

        /// <summary>
        /// Creates an <see cref="XmlReader" /> for reading an Atom feed from
        /// the provided <see cref="Uri" />.
        /// </summary>
        /// <param name="href">
        /// The relative <see cref="Uri" /> of the Atom feed to read.
        /// </param>
        /// <returns>
        /// An <see cref="XmlReader" /> over the Atom feed identified by
        /// <paramref name="href" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method attempts to find a BLOB corresponding to
        /// <paramref name="href" />. If the BLOB is found, an
        /// <see cref="XmlReader" /> over that BLOB is created and returned. If
        /// the BLOB isn't found, an XmlReader over an empty Atom Feed is
        /// returned. In this case, no BLOB is created for the empty Atom Feed.
        /// In other words: this method has no observable side-effects.
        /// </para>
        /// </remarks>
        public XmlReader CreateFeedReaderFor(Uri href)
        {
            var blobRef = this.CreateBlobReference(href);
            if (blobRef.Exists())
                return XmlReader.Create(
                    blobRef.OpenRead(),
                    new XmlReaderSettings { CloseInput = true });

            return AtomEventStorage.CreateNewFeed(href);
        }

        /// <summary>
        /// Creates an <see cref="XmlWriter" /> for writing the provided
        /// <see cref="AtomFeed" />.
        /// </summary>
        /// <param name="atomFeed">The Atom feed to write.</param>
        /// <returns>
        /// An <see cref="XmlWriter" /> over the Atom feed provided by
        /// <paramref name="atomFeed" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="atomFeed" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method returns an <see cref="XmlWriter" /> for a particular
        /// Atom Feed. The BLOB name and location is based on the Atom Feed's
        /// "self" link.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Returns an enumerator that iterates through all the event stream
        /// IDs in the BLOB container.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that
        /// can be used to iterate through the IDs.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The BLOB container can contain multiple event streams, each
        /// identified by an event stream ID. This Iterator enumerates all the
        /// event stream IDs. A client can use this to find all the IDs in the
        /// collection represented by this AtomEventsOnAzure instance.
        /// </para>
        /// </remarks>
        /// <seealso cref="AtomEventObserver{T}" />
        /// <seealso cref="FifoEvents{T}" />
        /// <seealso cref="LifoEvents{T}" />
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

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can
        /// be used to iterate through the collection.
        /// </returns>
        /// <seealso cref="GetEnumerator()" />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
