using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class XmlAtomContent
    {
        private readonly object item;

        public XmlAtomContent(object item)
        {
            this.item = item;
        }

        public object Item
        {
            get { return this.item; }
        }
    }
}
