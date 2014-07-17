using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests.SubNs.SubSubNs
{
    public class TestEventS
    {
        private readonly int number;
        private readonly string text;

        public TestEventS(int number, string text)
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
            var other = obj as TestEventS;
            if (other != null)
                return object.Equals(this.number, other.number)
                    && object.Equals(this.text, other.text);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
