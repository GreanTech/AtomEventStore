using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    public class TestEventD
    {
        private readonly int number;
        private readonly DateTimeOffset dateTime;

        public TestEventD(int number, DateTimeOffset dateTime)
        {
            this.number = number;
            this.dateTime = dateTime;
        }

        public int Number
        {
            get { return this.number; }
        }

        public DateTimeOffset DateTime
        {
            get { return this.dateTime; }
        }

        public TestEventD WithDateTime(DateTimeOffset newDateTime)
        {
            return new TestEventD(this.number, newDateTime);
        }

        public override bool Equals(object obj)
        {
            var other = obj as TestEventD;
            if (other != null)
                return object.Equals(this.number, other.number)
                    && object.Equals(this.dateTime, other.dateTime);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return
                this.number.GetHashCode() ^
                this.dateTime.GetHashCode();
        }
    }
}
