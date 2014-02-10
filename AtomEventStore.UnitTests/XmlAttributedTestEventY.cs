using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Grean.AtomEventStore.UnitTests
{
    [XmlRoot("test-event-y", Namespace = "http://grean:rocks")]
    public class XmlAttributedTestEventY
    {
        [XmlElement("number")]
        public decimal Number { get; set; }

        [XmlElement("flag")]
        public bool Flag { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as XmlAttributedTestEventY;
            if (other == null)
            return base.Equals(obj);

            return object.Equals(this.Number, other.Number)
                && object.Equals(this.Flag, other.Flag);
        }

        public override int GetHashCode()
        {
            return
                this.Number.GetHashCode() ^
                this.Flag.GetHashCode();
        }
    }
}
