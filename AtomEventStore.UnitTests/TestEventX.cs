using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public class TestEventX
    {
        private readonly int number;
        private readonly string text;

        public TestEventX(int number, string text)
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
    }
}
