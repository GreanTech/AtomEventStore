using Ploeh.AutoFixture.Idioms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventObserverTests
    {
        [Theory, AutoAtomData]
        public void PropertiesAreCorrectlyInitialized(
            ConstructorInitializedMemberAssertion assertion)
        {
            assertion.Verify(typeof(AtomEventObserver<TestEventX>));
        }
    }
}
