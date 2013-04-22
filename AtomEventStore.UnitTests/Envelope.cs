using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public class Envelope<T>
    {
        private readonly Guid id;
        private readonly T item;

        public Envelope(Guid id, T item)
        {
            this.id = id;
            this.item = item;
        }

        public Guid Id
        {
            get { return this.id; }
        }

        public T Item
        {
            get { return this.item; }
        }

        public override bool Equals(object obj)
        {
            if (obj is Envelope<T>)
            {
                var other = (Envelope<T>)obj;
                return object.Equals(this.id, other.id)
                    && object.Equals(this.item, other.item);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
