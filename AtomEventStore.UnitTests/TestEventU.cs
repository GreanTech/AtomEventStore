using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
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
    }
}
