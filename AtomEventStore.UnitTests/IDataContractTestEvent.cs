using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public interface IDataContractTestEvent
    {
        IDataContractTestEventVisitor Accept(IDataContractTestEventVisitor visitor);
    }
}
