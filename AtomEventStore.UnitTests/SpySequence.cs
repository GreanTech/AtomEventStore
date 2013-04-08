using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public class SpySequence
    {
        private int lastExpectedIndex;
        private int lastObservedIndex;
        private bool isUnordered;

        internal int GetNextExpectedIndex()
        {
            return ++this.lastExpectedIndex;
        }

        internal void Register(int i)
        {
            if (i <= this.lastObservedIndex)
                this.isUnordered = true;
            this.lastObservedIndex = i;
        }

        public bool IsOrdered
        {
            get { return !this.isUnordered; }
        }
    }
}
