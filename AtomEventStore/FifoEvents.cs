using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class FifoEvents<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            yield break;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
