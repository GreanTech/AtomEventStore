using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class SyndicationEventStream<T>
    {
        private readonly UuidIri id;
        private readonly IAtomEventPersistence persistence;

        public SyndicationEventStream(
            UuidIri id,
            IAtomEventPersistence persistence)
        {
            this.id = id;
            this.persistence = persistence;
        }

        public UuidIri Id
        {
            get { return this.id; }
        }

        public IAtomEventPersistence Persistence
        {
            get { return this.persistence; }
        }
    }
}
