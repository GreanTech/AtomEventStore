using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Grean.AtomEventStore.UnitTests
{
    [XmlRoot("test-event", Namespace = "http://grean:rocks")]
    public class XmlAttributedTestEventX
    {
        [XmlElement("number")]
        public int Number { get; set; }

        [XmlElement("text")]
        public string Text { get; set; }
    }
}
