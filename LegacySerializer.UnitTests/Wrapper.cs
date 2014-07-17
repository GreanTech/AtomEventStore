using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    public class Wrapper<T>
    {
        private readonly T item;

        public Wrapper(T item)
        {
            this.item = item;
        }

        public T Item
        {
            get { return this.item; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as Wrapper<T>;
            if (other == null)
                return base.Equals(obj);

            return object.Equals(this.item, other.item);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
