using System;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Implements a storage mechanism for use with
    /// <see cref="AtomEventObserver{T}" /> and <see cref="FifoEvents{T}" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="AtomEventObserver{T}" /> and <see cref="FifoEvents{T}" />
    /// implement event streams using the Atom syndication standard as a
    /// storage format. The responsibility of AtomEventObserver&lt;T&gt; and
    /// FifoEvents&lt;T&gt; is to format and interpret Atom feeds and entries
    /// so that they can be used as an event stream. However, the
    /// responsibility of actually writing and reading the Atom data falls to
    /// implementations of IAtomEventStorage.
    /// </para>
    /// <para>
    /// Implementations could, for example, persist Atom data on files, in rows
    /// in a relational database, in a cloud storage service, in a document
    /// database, or many other options. To keep the interface as flexible as
    /// possible, the API is mainly expressed in terms of
    /// <see cref="XmlReader" /> and <see cref="XmlWriter" />.
    /// </para>
    /// </remarks>
    /// <seealso cref="AtomEventsInFiles" />
    /// <seealso cref="AtomEventsInMemory" />
    /// <seealso cref="AtomEventObserver{T}" />
    /// <seealso cref="FifoEvents{T} "/>
    public interface IAtomEventStorage
    {
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
        /// <strong>Note to implementers:</strong>
        /// </para>
        /// <para>
        /// If no entry can be found for <paramref name="href" />, the method
        /// must return an <see cref="XmlReader" /> over an empty Atom feed.
        /// Returning <see langword="null" /> is considered an incorrect
        /// implementation.
        /// </para>
        /// <para>
        /// One, relatively easy, way to create an XmlReader over an empty Atom
        /// feed is to invoke
        /// <see cref="AtomEventStorage.CreateNewFeed(Uri)" />.
        /// </para>
        /// </remarks>
        XmlReader CreateFeedReaderFor(Uri href);

        /// <summary>
        /// Creates an <see cref="XmlWriter" /> for writing the provided
        /// <see cref="AtomFeed" />.
        /// </summary>
        /// <param name="atomFeed">The Atom feed to write.</param>
        /// <returns>
        /// An <see cref="XmlWriter" /> over the Atom feed provided by
        /// <paramref name="atomFeed" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Note to implementers:</strong>
        /// </para>
        /// <para>
        /// The implementation is free to choose an appropriate naming or
        /// identification scheme that fits the underlying persistence
        /// technology. However, it must be able to find the written Atom feed
        /// when a client subsequently invokes
        /// <see cref="CreateFeedReaderFor" />.
        /// </para>
        /// <para>
        /// When the CreateFeedWriterFor method is invoked,
        /// <paramref name="atomFeed" /> contains at least a 'self' link
        /// (identified by <see cref="AtomLink.IsSelfLink" />) identifying the
        /// Atom entry. The <see cref="AtomLink.Href" /> value of this link is
        /// the value used when CreateFeedReaderFor is subsequently invoked to
        /// read the Atom feed. In other words, this is the corrolation ID,
        /// so a naming or identification scheme must take this into account.
        /// </para>
        /// </remarks>
        XmlWriter CreateFeedWriterFor(AtomFeed atomFeed);
    }
}
