using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests.Demo.Visitor
{
    [DataContract(Name = "email-verified", Namespace = "urn:grean:samples:user-sign-up")]
    public class EmailVerified : IUserEvent
    {
        [DataMember(Name = "user-id")]
        public Guid UserId { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        public IUserVisitor Accept(IUserVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
