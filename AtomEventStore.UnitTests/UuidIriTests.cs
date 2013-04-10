using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;

namespace Grean.AtomEventStore.UnitTests
{
    public class UuidIriTests
    {
        [Theory, AutoAtomData]
        public void ToStringReturnsCorrectResult([Frozen]Guid g, UuidIri sut)
        {
            var actual = sut.ToString();
            Assert.Equal("urn:uuid:" + g, actual);
        }
    }
}
