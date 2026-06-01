using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Grean.AtomEventStore;

namespace Grean.AtomEventStore.AzureBlobAsync
{
    /// <summary>
    /// Stores events in Atom Feeds as blobs in Microsoft Azure, using the
    /// modern <c>Azure.Storage.Blobs</c> (v12) SDK and the asynchronous
    /// <see cref="IAsyncAtomEventStorage" /> contract.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a reference/sample implementation, intended to be copied or
    /// adapted rather than depended upon directly. It is the modern,
    /// asynchronous counterpart to the legacy <c>AtomEventsOnAzure</c> adapter.
    /// </para>
    /// <para>
    /// Authentication and endpoint configuration are the caller's
    /// responsibility: construct a <see cref="BlobContainerClient" /> however
    /// suits the application (connection string, SAS, <c>DefaultAzureCredential</c>,
    /// etc.) and pass it in. A single instance can manage several event streams
    /// within one container.
    /// </para>
    /// </remarks>
    /// <seealso cref="IAsyncAtomEventStorage" />
    /// <seealso cref="AsyncAtomEventObserver{T}" />
    /// <seealso cref="AsyncFifoEvents{T}" />
    public class AtomEventsOnAzureAsync :
        IAsyncAtomEventStorage, IAsyncEnumerable<UuidIri>
    {
        private const string xmlContentType = "application/xml";
        private readonly BlobContainerClient container;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AtomEventsOnAzureAsync" /> class.
        /// </summary>
        /// <param name="container">
        /// The blob container in which all blobs written by this instance will
        /// reside. The caller owns its authentication and lifetime.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="container" /> is <see langword="null" />.
        /// </exception>
        public AtomEventsOnAzureAsync(BlobContainerClient container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            this.container = container;
        }

        /// <summary>
        /// Asynchronously creates an <see cref="XmlReader" /> for reading an
        /// Atom feed from the provided <see cref="Uri" />.
        /// </summary>
        /// <param name="href">
        /// The relative <see cref="Uri" /> of the Atom feed to read.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that, when completed, contains an <see cref="XmlReader" />
        /// over the Atom feed identified by <paramref name="href" />. If no blob
        /// is found, the reader is over an empty Atom feed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="href" /> is <see langword="null" />.
        /// </exception>
        public Task<XmlReader> CreateFeedReaderForAsync(
            Uri href,
            CancellationToken cancellationToken)
        {
            // Validate synchronously (fail fast) before the async state machine.
            if (href == null)
                throw new ArgumentNullException("href");

            return this.CreateFeedReaderForAsyncImpl(href, cancellationToken);
        }

        private async Task<XmlReader> CreateFeedReaderForAsyncImpl(
            Uri href,
            CancellationToken cancellationToken)
        {
            var blob = this.container.GetBlobClient(BlobName(href));

            var exists = await blob.ExistsAsync(cancellationToken)
                .ConfigureAwait(false);
            if (!exists.Value)
                return AtomEventStorage.CreateNewFeed(href);

            var buffer = new MemoryStream();
            await blob.DownloadToAsync(buffer, cancellationToken)
                .ConfigureAwait(false);
            buffer.Position = 0;
            return XmlReader.Create(
                buffer,
                new XmlReaderSettings { CloseInput = true });
        }

        /// <summary>
        /// Asynchronously persists the rendered content of the provided
        /// <see cref="AtomFeed" /> as a blob.
        /// </summary>
        /// <param name="atomFeed">The Atom feed being written.</param>
        /// <param name="content">
        /// The already-rendered XML representation of the feed.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="atomFeed" /> or <paramref name="content" /> is
        /// <see langword="null" />.
        /// </exception>
        public Task WriteFeedAsync(
            AtomFeed atomFeed,
            string content,
            CancellationToken cancellationToken)
        {
            // Validate synchronously (fail fast) before the async state machine.
            if (atomFeed == null)
                throw new ArgumentNullException("atomFeed");
            if (content == null)
                throw new ArgumentNullException("content");

            return this.WriteFeedAsyncImpl(atomFeed, content, cancellationToken);
        }

        private async Task WriteFeedAsyncImpl(
            AtomFeed atomFeed,
            string content,
            CancellationToken cancellationToken)
        {
            var selfLink = atomFeed.Links.Single(l => l.IsSelfLink);
            var blob = this.container.GetBlobClient(BlobName(selfLink.Href));

            var bytes = Encoding.UTF8.GetBytes(content);
            using (var source = new MemoryStream(bytes))
            {
                var options = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = xmlContentType
                    }
                };
                await blob.UploadAsync(source, options, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private static string BlobName(Uri href)
        {
            return href.ToString() + ".xml";
        }

        /// <summary>
        /// Returns an asynchronous enumerator over all the event stream IDs in
        /// the blob container.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the enumeration.
        /// </param>
        /// <returns>
        /// An asynchronous enumerator over the event stream IDs.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Each event stream's blobs reside under a virtual directory named by
        /// the stream's ID (a <see cref="Guid" />). This enumerator lists those
        /// top-level virtual directories (using "/" as a delimiter) and yields
        /// the ones whose name is a valid stream ID.
        /// </para>
        /// </remarks>
        public async IAsyncEnumerator<UuidIri> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            var pages = this.container.GetBlobsByHierarchyAsync(
                delimiter: "/",
                cancellationToken: cancellationToken);

            await foreach (var item in pages.ConfigureAwait(false))
            {
                if (!item.IsPrefix)
                    continue;

                var name = item.Prefix.Trim('/');
                if (Guid.TryParse(name, out var id) && id != Guid.Empty)
                    yield return (UuidIri)id;
            }
        }
    }
}
