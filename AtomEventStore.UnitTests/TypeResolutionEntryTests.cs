using Ploeh.AutoFixture.Idioms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;

namespace Grean.AtomEventStore.UnitTests
{
    public class TypeResolutionEntryTests
    {
        [Theory, AutoAtomData]
        public void VerifyConstructorInitializedProperties(
            ConstructorInitializedMemberAssertion assertion)
        {
            assertion.Verify(typeof(TypeResolutionEntry).GetProperties());
        }
    }
}
