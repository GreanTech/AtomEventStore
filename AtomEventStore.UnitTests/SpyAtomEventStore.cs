using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore.UnitTests
{
    public class SpyAtomEventStore : IAtomEventStorage
    {
        private readonly IAtomEventStorage store;
        private readonly List<object> observedArguments;

        public SpyAtomEventStore()
        {
            this.store = new AtomEventsInMemory();
            this.observedArguments = new List<object>();
        }

        public IEnumerable<object> ObservedArguments
        {
            get { return this.observedArguments; }
        }

        public XmlReader CreateFeedReaderFor(Uri href)
        {
            this.observedArguments.Add(href);
            return this.store.CreateFeedReaderFor(href);
        }

        public XmlWriter CreateFeedWriterFor(AtomFeed atomFeed)
        {
            this.observedArguments.Add(atomFeed);
            return this.store.CreateFeedWriterFor(atomFeed);
        }
    }
}
