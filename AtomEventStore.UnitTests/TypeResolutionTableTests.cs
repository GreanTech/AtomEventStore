using Ploeh.AutoFixture.Idioms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace Grean.AtomEventStore.UnitTests
{
    public class TypeResolutionTableTests
    {
        [Fact]
        public void SutIsTypeResolver()
        {
            var sut = new TypeResolutionTable();
            Assert.IsAssignableFrom<ITypeResolver>(sut);
        }

        [Theory, AutoAtomData]
        public void VerifyGuardClauses(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(TypeResolutionTable));
        }
    }
}
