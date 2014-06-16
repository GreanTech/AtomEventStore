using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    [DataContract(Name = "envelope", Namespace = "http://grean.rocks/dc")]
    [KnownType(typeof(DataContractTestEventX))]
    [KnownType(typeof(DataContractTestEventY))]
    public class DataContractEnvelope<T> where T : IDataContractTestEvent
    {
        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        [DataMember(Name = "item")]
        public T Item { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is DataContractEnvelope<T>)
            {
                var other = (DataContractEnvelope<T>)obj;
                return object.Equals(this.Id, other.Id)
                    && object.Equals(this.Item, other.Item);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public DataContractEnvelope<TResult> Cast<TResult>() where TResult : IDataContractTestEvent
        {
            return new DataContractEnvelope<TResult>
            {
                Id = this.Id,
                Item = (TResult)(object)this.Item
            };
        }
    }
}
