using System.Xml;

namespace Grean.AtomEventStore
{
    public interface IAtomEventPersistence
    {
        XmlReader CreateEntryReaderFor(UuidIri id);
        XmlWriter CreateEntryWriterFor(AtomEntry atomEntry);
        XmlReader CreateFeedReaderFor(UuidIri id);
        XmlWriter CreateFeedWriterFor(AtomFeed atomFeed);
    }
}
