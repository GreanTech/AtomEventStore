using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class FifoEventsTests
    {
        [Theory, AutoAtomData]
        public void SutIsEnumerable(FifoEvents<XmlAttributedTestEventX> sut)
        {
            Assert.IsAssignableFrom<IEnumerable<XmlAttributedTestEventX>>(sut);
        }

        [Theory, AutoAtomData]
        public void PropertiesAreCorrectlyInitialized(
            ConstructorInitializedMemberAssertion assertion)
        {
            assertion.Verify(typeof(FifoEvents<XmlAttributedTestEventX>));
        }

        [Theory, AutoAtomData]
        public void SutIsInitiallyEmpty(
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            FifoEvents<XmlAttributedTestEventX> sut)
        {
            Assert.False(sut.Any(), "Intial event stream should be empty.");
            Assert.Empty(sut);
        }
    }
}
