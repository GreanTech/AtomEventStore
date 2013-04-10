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

        [Theory, AutoAtomData]
        public void SutCorrectlyConvertsToGuid(
            [Frozen]Guid expected,
            UuidIri sut)
        {
            Guid actual = sut;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void GuidCorrectlyConvertsToSut(
            UuidIri sut)
        {
            Guid expected = sut;
            UuidIri actual = expected;
            Assert.Equal<Guid>(expected, actual);
        }

        [Theory, AutoAtomData]
        public void TwoSutsWithIdenticalIdAreEqual(Guid guid)
        {
            UuidIri sut = guid;
            UuidIri other = guid;

            var actual = sut.Equals(other);

            Assert.True(actual, "Equals");
        }

        [Theory, AutoAtomData]
        public void TwoSutsWithDifferentIdsAreNotEquals(
            Guid x,
            Guid y)
        {
            Assert.NotEqual(x, y);

            UuidIri sut = x;
            UuidIri other = y;

            var actual = sut.Equals(other);

            Assert.False(actual, "Equals");
        }
    }
}
