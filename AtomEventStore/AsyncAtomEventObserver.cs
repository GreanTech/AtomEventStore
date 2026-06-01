using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Asynchronously writes events to a specific event stream.
    /// </summary>
    /// <typeparam name="T">The type of the events.</typeparam>
    /// <remarks>
    /// <para>
    /// This is the genuinely-asynchronous counterpart to
    /// <see cref="AtomEventObserver{T}" />. It uses the same Atom linked-list
    /// storage approach, but performs its I/O through
    /// <see cref="IAsyncAtomEventStorage" />, awaiting reads and writes rather
    /// than blocking. Serialization remains synchronous (the underlying
    /// serializers expose no async API); only the I/O boundary is asynchronous.
    /// </para>
    /// <para>
    /// Read the stream with <see cref="AsyncFifoEvents{T}" /> or
    /// <see cref="AsyncLifoEvents{T}" />.
    /// </para>
    /// </remarks>
    /// <seealso cref="AtomEventObserver{T}" />
    /// <seealso cref="AsyncFifoEvents{T}" />
    /// <seealso cref="IAsyncAtomEventStorage" />
    public class AsyncAtomEventObserver<T>
    {
        private readonly UuidIri id;
        private readonly int pageSize;
        private readonly IAsyncAtomEventStorage storage;
        private readonly IContentSerializer serializer;

        private static readonly XmlWriterSettings writerSettings =
            new XmlWriterSettings { OmitXmlDeclaration = true };

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AsyncAtomEventObserver{T}" /> class.
        /// </summary>
        /// <param name="id">The ID of the event stream.</param>
        /// <param name="pageSize">
        /// The maximum number of instances of T stored in a single Atom feed
        /// page.
        /// </param>
        /// <param name="storage">
        /// The underlying asynchronous storage mechanism to use.
        /// </param>
        /// <param name="serializer">
        /// The serializer used to serialize and deserialize items to a format
        /// compatible with Atom.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="storage" /> or <paramref name="serializer" /> is
        /// <see langword="null" />.
        /// </exception>
        public AsyncAtomEventObserver(
            UuidIri id,
            int pageSize,
            IAsyncAtomEventStorage storage,
            IContentSerializer serializer)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            this.id = id;
            this.pageSize = pageSize;
            this.storage = storage;
            this.serializer = serializer;
        }

        /// <summary>
        /// Appends an event to the event stream.
        /// </summary>
        /// <param name="event">The event to append.</param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous append.
        /// </returns>
        public Task AppendAsync(T @event)
        {
            return this.AppendAsync(@event, CancellationToken.None);
        }

        /// <summary>
        /// Appends an event to the event stream.
        /// </summary>
        /// <param name="event">The event to append.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous append.
        /// </returns>
        public async Task AppendAsync(T @event, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;

            var ctx = await this.PrepareAsync(now, cancellationToken)
                .ConfigureAwait(false);

            var entry = CreateEntry(@event, now);

            if (this.PageSizeReached(ctx.LastPage))
                await this.WriteEntryToNewPageAsync(entry, ctx, cancellationToken)
                    .ConfigureAwait(false);
            else
                await this.WriteEntryToExistingPageAsync(entry, ctx, cancellationToken)
                    .ConfigureAwait(false);
        }

        private async Task<AppendContext> PrepareAsync(
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var index = await this.ReadIndexAsync(cancellationToken)
                .ConfigureAwait(false);
            var firstLink = index.Links
                .Where(l => l.IsFirstLink)
                .DefaultIfEmpty(AtomLink.CreateFirstLink(this.CreateNewFeedAddress()))
                .Single();
            index = index.WithLinks(index.Links.Union(new[] { firstLink }));

            var lastLink = index.Links.SingleOrDefault(l => l.IsLastLink);
            var lastLinkAdded = false;
            if (lastLink == null)
            {
                lastLink = firstLink.ToLastLink();
                lastLinkAdded = true;
            }
            var lastPage = await this.ReadTrueLastPageAsync(lastLink.Href, cancellationToken)
                .ConfigureAwait(false);
            var lastLinkCorrected = false;
            if (lastPage.Links.Single(l => l.IsSelfLink).Href != lastLink.Href)
            {
                lastLink = lastPage.Links.Single(l => l.IsSelfLink).ToLastLink();
                lastLinkCorrected = true;
            }
            index = index.WithLinks(index.Links
                .Where(l => !l.IsLastLink)
                .Concat(new[] { lastLink }));

            return new AppendContext(
                index,
                lastPage,
                now,
                lastLinkAdded,
                lastLinkCorrected);
        }

        private sealed class AppendContext
        {
            public readonly AtomFeed Index;
            public readonly AtomFeed LastPage;
            public readonly DateTimeOffset Now;
            public readonly bool LastLinkAdded;
            public readonly bool LastLinkCorrected;

            public AppendContext(
                AtomFeed index,
                AtomFeed lastPage,
                DateTimeOffset now,
                bool lastLinkAdded,
                bool lastLinkCorrected)
            {
                this.Index = index;
                this.LastPage = lastPage;
                this.Now = now;
                this.LastLinkAdded = lastLinkAdded;
                this.LastLinkCorrected = lastLinkCorrected;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The index update is a best-effort optimization behind the IAsyncAtomEventStorage interface, so the specific exception type isn't known. As in the synchronous AtomEventObserver, suppressing any failure here is intentional.")]
        private async Task WriteEntryToNewPageAsync(
            AtomEntry entry,
            AppendContext context,
            CancellationToken cancellationToken)
        {
            var newAddress = this.CreateNewFeedAddress();
            var newPage = await this.ReadPageAsync(newAddress, cancellationToken)
                .ConfigureAwait(false);
            newPage = AddEntryTo(newPage, entry, context.Now);

            var nextLink = AtomLink.CreateNextLink(newAddress);

            var previousPage = context.LastPage
                .WithLinks(context.LastPage.Links.Concat(new[] { nextLink }));

            var previousLink = previousPage.Links
                .Single(l => l.IsSelfLink)
                .ToPreviousLink();

            newPage = newPage.WithLinks(
                newPage.Links.Concat(new[] { previousLink }));
            var index = context.Index.WithLinks(context.Index.Links
                .Where(l => !l.IsLastLink)
                .Concat(new[] { nextLink.ToLastLink() }));

            await this.WriteAsync(newPage, cancellationToken).ConfigureAwait(false);
            await this.WriteAsync(previousPage, cancellationToken).ConfigureAwait(false);
            try { await this.WriteAsync(index, cancellationToken).ConfigureAwait(false); }
            catch { }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The index update is a best-effort optimization behind the IAsyncAtomEventStorage interface, so the specific exception type isn't known. As in the synchronous AtomEventObserver, suppressing any failure here is intentional.")]
        private async Task WriteEntryToExistingPageAsync(
            AtomEntry entry,
            AppendContext context,
            CancellationToken cancellationToken)
        {
            var lastPage = AddEntryTo(context.LastPage, entry, context.Now);

            await this.WriteAsync(lastPage, cancellationToken).ConfigureAwait(false);
            if (context.LastLinkAdded)
                await this.WriteAsync(context.Index, cancellationToken).ConfigureAwait(false);
            else if (context.LastLinkCorrected)
                try { await this.WriteAsync(context.Index, cancellationToken).ConfigureAwait(false); }
                catch { }
        }

        private Uri CreateNewFeedAddress()
        {
            var indexedAddress = ((Guid)this.id) + "/" + Guid.NewGuid();
            return new Uri(indexedAddress, UriKind.Relative);
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

        private async Task<AtomFeed> ReadTrueLastPageAsync(
            Uri address,
            CancellationToken cancellationToken)
        {
            var page = await this.ReadPageAsync(address, cancellationToken)
                .ConfigureAwait(false);
            var nextLink = page.Links.SingleOrDefault(l => l.IsNextLink);
            while (nextLink != null)
            {
                page = await this.ReadPageAsync(nextLink.Href, cancellationToken)
                    .ConfigureAwait(false);
                nextLink = page.Links.SingleOrDefault(l => l.IsNextLink);
            }

            return page;
        }

        private static AtomEntry CreateEntry(T @event, DateTimeOffset now)
        {
            var changesetId = Guid.NewGuid();
            return new AtomEntry(
                changesetId,
                "Changeset " + changesetId,
                now,
                now,
                new AtomAuthor("Grean"),
                new XmlAtomContent(@event),
                Array.Empty<AtomLink>());
        }

        private static AtomFeed AddEntryTo(
            AtomFeed page,
            AtomEntry entry,
            DateTimeOffset now)
        {
            var entries = new[] { entry }.Concat(page.Entries);
            return page
                .WithEntries(entries)
                .WithUpdated(now);
        }

        private Task WriteAsync(AtomFeed feed, CancellationToken cancellationToken)
        {
            var content = feed.ToXmlString(this.serializer, writerSettings);
            return this.storage.WriteFeedAsync(feed, content, cancellationToken);
        }

        private bool PageSizeReached(AtomFeed page)
        {
            return page.Entries.Count() >= this.pageSize;
        }

        /// <summary>
        /// Gets the ID of the event stream.
        /// </summary>
        public UuidIri Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the maximum page size.
        /// </summary>
        public int PageSize
        {
            get { return this.pageSize; }
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
