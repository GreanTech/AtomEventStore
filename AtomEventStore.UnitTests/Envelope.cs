using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    [TypeConverter(typeof(EnvelopeTypeConverter))]
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

        public Envelope<TResult> Cast<TResult>()
        {
            return new Envelope<TResult>(
                this.id,
                (TResult)(object)this.item);
        }
    }

    /// <summary>
    /// This class mostly exists to test for proper type resolution behavior
    /// when resolving type with ambigous names. This non-generic type has the
    /// same name as Envelope&lt;T&gt;, but is a static class, so should not be
    /// picked.
    /// </summary>
    public static class Envelope
    {
        public static Envelope<T> Envelop<T>(this T item)
        {
            return new Envelope<T>(
                Guid.NewGuid(),
                item);
        }
    }
}
