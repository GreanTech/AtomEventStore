using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class AutoAtomMoqDataAttribute : AutoDataAttribute
    {
        public AutoAtomMoqDataAttribute()
            : base(new Fixture().Customize(new MoqTestConventions()))
        {
        }
    }
}
