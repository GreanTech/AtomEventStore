using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests.Demo.Visitor
{
    public interface IUserVisitor
    {
        IUserVisitor Visit(UserCreated @event);

        IUserVisitor Visit(EmailVerified @event);

        IUserVisitor Visit(EmailChanged @event);
    }
}
