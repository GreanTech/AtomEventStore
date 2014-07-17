using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    public sealed class TestEventSealed
    {
        private readonly int number;
        private readonly string text;

        public TestEventSealed(int number, string text)
        {
            this.number = number;
            this.text = text;
        }

        public int Number
        {
            get { return this.number; }
        }

        public string Text
        {
            get { return this.text; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TestEventSealed;
            if (other != null)
                return object.Equals(this.number, other.number)
                    && object.Equals(this.text, other.text);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return
                this.number.GetHashCode() ^
                this.text.GetHashCode();
        }
    }
}
