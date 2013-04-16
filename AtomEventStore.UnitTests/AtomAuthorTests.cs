using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Xunit;
using Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomAuthorTests
    {
        [Theory, AutoAtomData]
        public void NameIsCorrectWhenModestConstructorIsUsed(
            [Frozen]string expected,
            [Modest]AtomAuthor sut)
        {
            string actual = sut.Name;
            Assert.Equal(expected, actual);
        }
    }
}
