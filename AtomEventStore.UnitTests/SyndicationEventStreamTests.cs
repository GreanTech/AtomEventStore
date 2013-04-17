using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class SyndicationEventStreamTests
    {
        [Theory, AutoAtomData]
        public void IdIsCorrect(
            [Frozen]UuidIri expected,
            SyndicationEventStream<TestEventX> sut)
        {
            UuidIri actual = sut.Id;
            Assert.Equal(expected, actual);
        }
    }
}
