using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    public interface ITestEvent
    {
        ITestEventVisitor Accept(ITestEventVisitor visitor);
    }
}
