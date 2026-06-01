using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Implements an asynchronous storage mechanism for use with
    /// <see cref="AsyncAtomEventObserver{T}" />,
    /// <see cref="AsyncFifoEvents{T}" /> and
    /// <see cref="AsyncLifoEvents{T}" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the asynchronous counterpart to <see cref="IAtomEventStorage" />.
    /// It exists alongside the synchronous interface (rather than replacing it),
    /// so existing consumers are unaffected. Built-in implementations
    /// (<see cref="AtomEventsInMemory" /> and <see cref="AtomEventsInFiles" />)
    /// implement both interfaces.
    /// </para>
    /// <para>
    /// The asynchrony is at the I/O boundary only: reading produces an
    /// <see cref="XmlReader" /> over content fetched asynchronously, and writing
    /// persists already-rendered content asynchronously. Parsing and
    /// serialization remain synchronous, because the underlying serializers
    /// (<see cref="System.Runtime.Serialization.DataContractSerializer" /> and
    /// <see cref="System.Xml.Serialization.XmlSerializer" />) expose no async
    /// API. Keeping storage agnostic of serialization is deliberate: the caller
    /// renders the feed to a <c>content</c> string and storage merely persists
    /// it by the feed's "self" link.
    /// </para>
    /// </remarks>
    /// <seealso cref="IAtomEventStorage" />
    /// <seealso cref="AtomEventsInMemory" />
    /// <seealso cref="AtomEventsInFiles" />
    public interface IAsyncAtomEventStorage
    {
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
        /// over the Atom feed identified by <paramref name="href" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Note to implementers:</strong> if no entry can be found for
        /// <paramref name="href" />, the returned <see cref="XmlReader" /> must
        /// be over an empty Atom feed (e.g. via
        /// <see cref="AtomEventStorage.CreateNewFeed(Uri)" />). Returning
        /// <see langword="null" /> is considered an incorrect implementation.
        /// </para>
        /// </remarks>
        Task<XmlReader> CreateFeedReaderForAsync(
            Uri href,
            CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously persists the rendered content of the provided
        /// <see cref="AtomFeed" />.
        /// </summary>
        /// <param name="atomFeed">
        /// The Atom feed being written. Its "self" link
        /// (<see cref="AtomLink.IsSelfLink" />) identifies where the content is
        /// stored, so that a subsequent
        /// <see cref="CreateFeedReaderForAsync" /> can find it.
        /// </param>
        /// <param name="content">
        /// The already-rendered XML representation of
        /// <paramref name="atomFeed" />. Storage persists this verbatim and
        /// does not interpret it, keeping the storage mechanism agnostic of the
        /// serialization format.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous write operation.
        /// </returns>
        Task WriteFeedAsync(
            AtomFeed atomFeed,
            string content,
            CancellationToken cancellationToken);
    }
}
