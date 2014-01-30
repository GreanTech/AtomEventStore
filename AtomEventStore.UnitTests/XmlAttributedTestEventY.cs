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
    }
}
