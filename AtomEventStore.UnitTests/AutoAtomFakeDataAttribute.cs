using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class AutoAtomFakeDataAttribute : AutoDataAttribute
    {
        public AutoAtomFakeDataAttribute()
            : base(new Fixture().Customize(new FakeTestConventions()))
        {
        }
    }
}
