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
    public class AtomEventStreamTests
    {
        [Theory, AutoAtomData]
        public void IdIsCorrect(
            [Frozen]UuidIri expected,
            AtomEventStream<TestEventX> sut)
        {
            UuidIri actual = sut.Id;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void PersistenceIsCorrect(
            [Frozen]IAtomEventPersistence expected,
            AtomEventStream<TestEventX> sut)
        {
            IAtomEventPersistence actual = sut.Persistence;
            Assert.Equal(expected, actual);
        }
    }
}
