using System;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Implements a storage mechanism for use with
    /// <see cref="AtomEventStream{T}" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="AtomEventStream{T}" /> implements an event stream using the
    /// Atom syndication standard as a storage format. The responsibility of
    /// AtomEventStream&lt;T&gt; is to format and interpret Atom feeds and
    /// entries so that they can be used as an event stream. However, the
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
    /// <seealso cref="AtomEventStream{T}" />
    public interface IAtomEventStorage
    {
        XmlReader CreateEntryReaderFor(Uri href);
        XmlWriter CreateEntryWriterFor(AtomEntry atomEntry);
        XmlReader CreateFeedReaderFor(Uri href);
        XmlWriter CreateFeedWriterFor(AtomFeed atomFeed);
    }
}
