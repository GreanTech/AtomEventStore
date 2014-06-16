using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Grean.AtomEventStore.UnitTests
{
    [DataContract(Name = "test-event-x", Namespace = "http://grean.rocks/dc")]
    public class DataContractTestEventX : IDataContractTestEvent
    {
        [DataMember(Name = "number")]
        public string Number { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as DataContractTestEventX;
            if (other != null)
                return object.Equals(this.Number, other.Number)
                    && object.Equals(this.Text, other.Text);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return
                this.Number.GetHashCode() ^
                this.Text.GetHashCode();
        }

        public IDataContractTestEventVisitor Accept(
            IDataContractTestEventVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
