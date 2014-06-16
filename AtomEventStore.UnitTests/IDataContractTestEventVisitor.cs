using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public interface IDataContractTestEventVisitor
    {
        IDataContractTestEventVisitor Visit(DataContractTestEventX tex);

        IDataContractTestEventVisitor Visit(DataContractTestEventY tey);
    }
}
