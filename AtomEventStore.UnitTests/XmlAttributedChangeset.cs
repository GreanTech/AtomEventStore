using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Grean.AtomEventStore.UnitTests
{
    [XmlRoot("changeset", Namespace = "http://grean:rocks")]
    public class XmlAttributedChangeset
    {
        [XmlElement("id")]
        public Guid Id { get; set; }

        [XmlArray("items")]
        [XmlArrayItem("test-event-x", typeof(XmlAttributedTestEventX))]
        [XmlArrayItem("test-event-y", typeof(XmlAttributedTestEventY))]
        public object[] Items { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as XmlAttributedChangeset;
            if (other == null)
                return base.Equals(obj);

            return object.Equals(this.Id, other.Id)
                && this.Items.SequenceEqual(other.Items);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
