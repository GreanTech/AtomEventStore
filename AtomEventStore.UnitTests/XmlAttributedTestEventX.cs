using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Grean.AtomEventStore.UnitTests
{
    [XmlRoot("test-event-x", Namespace = "http://grean:rocks")]
    public class XmlAttributedTestEventX
    {
        [XmlElement("number")]
        public int Number { get; set; }

        [XmlElement("text")]
        public string Text { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as XmlAttributedTestEventX;
            if (other == null)
            return base.Equals(obj);

            return object.Equals(this.Number, other.Number)
                && object.Equals(this.Text, other.Text);
        }

        public override int GetHashCode()
        {
            return
                this.Number.GetHashCode() ^
                this.Text.GetHashCode();
        }
    }
}
