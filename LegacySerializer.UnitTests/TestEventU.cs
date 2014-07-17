using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    public class TestEventU
    {
        private readonly Uri address;
        private readonly string text;

        public TestEventU(Uri address, string text)
        {
            this.address = address;
            this.text = text;
        }

        public Uri Address
        {
            get { return this.address; }
        }

        public string Text
        {
            get { return this.text; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TestEventU;
            if (other != null)
                return object.Equals(this.address, other.address)
                    && object.Equals(this.text, other.text);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
