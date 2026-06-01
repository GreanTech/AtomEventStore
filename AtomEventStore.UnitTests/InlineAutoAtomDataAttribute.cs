using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture.Xunit2;

namespace Grean.AtomEventStore.UnitTests
{
    public class InlineAutoAtomDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoAtomDataAttribute(params object[] values)
            : base(new AutoAtomDataAttribute(), values)
        {
        }
    }
}
