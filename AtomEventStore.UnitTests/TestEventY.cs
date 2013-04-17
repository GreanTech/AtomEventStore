using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public class TestEventY
    {
        private readonly long number;
        private readonly bool isTrue;

        public TestEventY(long number, bool isTrue)
        {
            this.number = number;
            this.isTrue = isTrue;
        }

        public long Number
        {
            get { return this.number; }
        }

        public bool IsTrue
        {
            get { return this.isTrue; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TestEventY;
            if (other != null)
                return object.Equals(this.number, other.number)
                    && object.Equals(this.isTrue, other.isTrue);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return
                this.number.GetHashCode() ^
                this.isTrue.GetHashCode();
        }
    }
}
