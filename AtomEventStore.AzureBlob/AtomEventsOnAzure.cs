using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore.AzureBlob
{
    public class AtomEventsOnAzure : IAtomEventStorage
    {
        public XmlReader CreateEntryReaderFor(Uri href)
        {
            throw new NotImplementedException();
        }

        public XmlWriter CreateEntryWriterFor(AtomEntry atomEntry)
        {
            throw new NotImplementedException();
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            throw new NotImplementedException();
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            throw new NotImplementedException();
        }
    }
}
