using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests.Demo.Visitor
{
    public interface IUserEvent
    {
        IUserVisitor Accept(IUserVisitor visitor);
    }
}
