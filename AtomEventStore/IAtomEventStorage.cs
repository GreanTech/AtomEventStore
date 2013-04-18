using System;
using System.Xml;

namespace Grean.AtomEventStore
{
    public interface IAtomEventStorage
    {
        XmlReader CreateEntryReaderFor(Uri href);
        XmlWriter CreateEntryWriterFor(AtomEntry atomEntry);
        XmlReader CreateFeedReaderFor(Uri href);
        XmlWriter CreateFeedWriterFor(AtomFeed atomFeed);
    }
}
