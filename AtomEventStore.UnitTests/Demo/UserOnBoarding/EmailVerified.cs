using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests.Demo.UserOnBoarding
{
    [DataContract(Name = "email-verified", Namespace = "urn:grean:samples:user-on-boarding")]
    public class EmailVerified
    {
        [DataMember(Name = "user-id")]
        public Guid UserId { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }
    }
}
