using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests.Demo.AcyclicVisitor
{
    public class UserTypeResolver : ITypeResolver
    {
        public Type Resolve(string localName, string xmlNamespace)
        {
            switch (xmlNamespace)
            {
                case "urn:grean:samples:user-on-boarding":
                    switch (localName)
                    {
                        case "user-created":
                            return typeof(UserCreated);
                        case "email-verified":
                            return typeof(EmailVerified);
                        case "email-changed":
                            return typeof(EmailChanged);
                        default:
                            throw new ArgumentException(
                                "Unknown local name.",
                                "localName");
                    }
                default:
                    throw new ArgumentException(
                        "Unknown XML namespace.",
                        "xmlNamespace");
            }
        }
    }
}
