using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// A forward-moving, asynchronously-read sequence of events.
    /// </summary>
    /// <typeparam name="T">
    /// The type of event represented by the stream.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// This is the asynchronous counterpart to <see cref="FifoEvents{T}" />,
    /// exposing the stream as an <see cref="IAsyncEnumerable{T}" /> read through
    /// <see cref="IAsyncAtomEventStorage" />. As with the synchronous version,
    /// it reads ahead: while the caller consumes the entries of the current
    /// page, the next page is fetched concurrently.
    /// </para>
    /// <para>
    /// Use <see cref="AsyncAtomEventObserver{T}" /> to write the events.
    /// </para>
    /// </remarks>
    /// <seealso cref="FifoEvents{T}" />
    /// <seealso cref="AsyncLifoEvents{T}" />
    /// <seealso cref="AsyncAtomEventObserver{T}" />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suppressed following discussion at http://bit.ly/11T4eZe")]
    public class AsyncFifoEvents<T> : IAsyncEnumerable<T>
    {
        private readonly UuidIri id;
        private readonly IAsyncAtomEventStorage storage;
        private readonly IContentSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncFifoEvents{T}" />
        /// class.
        /// </summary>
        /// <param name="id">The ID of the event stream.</param>
        /// <param name="storage">
        /// The underlying asynchronous storage mechanism from which to read.
        /// </param>
        /// <param name="serializer">
        /// The serializer used to deserialize items from a format compatible
        /// with Atom.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="storage" /> or <paramref name="serializer" /> is
        /// <see langword="null" />.
        /// </exception>
        public AsyncFifoEvents(
            UuidIri id,
            IAsyncAtomEventStorage storage,
            IContentSerializer serializer)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            this.id = id;
            this.storage = storage;
            this.serializer = serializer;
        }

        /// <summary>
        /// Returns an asynchronous enumerator that iterates through the events,
        /// from earliest to most recent.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the enumeration.
        /// </param>
        /// <returns>An asynchronous enumerator over the events.</returns>
        public async IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            var page = await this.ReadFirstAsync(cancellationToken)
                .ConfigureAwait(false);
            while (page != null)
            {
                var entries = page.Entries.Reverse().ToArray();
                if (entries.Length == 0)
                    yield break;

                yield return (T)entries.First().Content.Item;

                // Read ahead: start fetching the next page while the caller
                // consumes the remaining entries of the current page.
                var nextTask = this.ReadNextAsync(page, cancellationToken);

                foreach (var entry in entries.Skip(1))
                    yield return (T)entry.Content.Item;

                page = await nextTask.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns the equivalent <see cref="AsyncLifoEvents{T}" /> instance.
        /// </summary>
        /// <returns>The equivalent <see cref="AsyncLifoEvents{T}" /> instance.</returns>
        public AsyncLifoEvents<T> Reverse()
        {
            return new AsyncLifoEvents<T>(this.id, this.storage, this.serializer);
        }

        private async Task<AtomFeed> ReadFirstAsync(CancellationToken cancellationToken)
        {
            var index = await this.ReadIndexAsync(cancellationToken)
                .ConfigureAwait(false);
            var firstLink = index.Links.SingleOrDefault(l => l.IsFirstLink);
            if (firstLink == null)
                return null;

            return await this.ReadPageAsync(firstLink.Href, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<AtomFeed> ReadNextAsync(
            AtomFeed page,
            CancellationToken cancellationToken)
        {
            var nextLink = page.Links.SingleOrDefault(l => l.IsNextLink);
            if (nextLink == null)
                return null;

            return await this.ReadPageAsync(nextLink.Href, cancellationToken)
                .ConfigureAwait(false);
        }

        private Task<AtomFeed> ReadIndexAsync(CancellationToken cancellationToken)
        {
            var indexAddress =
                new Uri(
                    ((Guid)this.id) + "/" + ((Guid)this.id),
                    UriKind.Relative);
            return this.ReadPageAsync(indexAddress, cancellationToken);
        }

        private async Task<AtomFeed> ReadPageAsync(
            Uri address,
            CancellationToken cancellationToken)
        {
            using (var r = await this.storage
                .CreateFeedReaderForAsync(address, cancellationToken)
                .ConfigureAwait(false))
                return AtomFeed.ReadFrom(r, this.serializer);
        }

        /// <summary>
        /// Gets the ID of the event stream.
        /// </summary>
        public UuidIri Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the underlying asynchronous storage mechanism.
        /// </summary>
        public IAsyncAtomEventStorage Storage
        {
            get { return this.storage; }
        }

        /// <summary>
        /// Gets the content serializer.
        /// </summary>
        public IContentSerializer Serializer
        {
            get { return this.serializer; }
        }
    }
}
