using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Grean.AtomEventStore.UnitTests
{
    [DataContract(Name = "test-event-x", Namespace = "http://grean.rocks/dc")]
    public class DataContractTestEventX
    {
        [DataMember(Name = "number")]
        public string Number { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }
    }
}
