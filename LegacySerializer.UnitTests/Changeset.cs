using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    public class Changeset<T> : IEnumerable<T>
    {
        private readonly Guid id;
        private readonly IEnumerable<T> items;

        public Changeset(Guid id, params T[] items)
        {
            this.id = id;
            this.items = items;
        }

        public Guid Id
        {
            get { return this.id; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as Changeset<T>;
            if (other == null)
                return base.Equals(obj);

            return object.Equals(this.id, other.id)
                && this.items.SequenceEqual(other.items);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
