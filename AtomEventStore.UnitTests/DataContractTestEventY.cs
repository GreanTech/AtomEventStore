using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    [DataContract(Name = "test-event-y", Namespace = "http://grean.rocks/dc")]
    public class DataContractTestEventY
    {
        [DataMember(Name = "number")]
        public long Number { get; set; }

        [DataMember(Name = "text")]
        public bool IsTrue { get; set; }
    }
}
