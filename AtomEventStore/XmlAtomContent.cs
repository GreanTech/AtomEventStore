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

        public XmlAtomContent WithItem(object newItem)
        {
            return new XmlAtomContent(newItem);
        }

        public override bool Equals(object obj)
        {
            var other = obj as XmlAtomContent;
            if (other != null)
                return object.Equals(this.item, other.item);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.item.GetHashCode();
        }
    }
}
