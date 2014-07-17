using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    public interface ITestEventVisitor
    {
        ITestEventVisitor Visit(TestEventX tex);

        ITestEventVisitor Visit(TestEventY tey);
    }
}
