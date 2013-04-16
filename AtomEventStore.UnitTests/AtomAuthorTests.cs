using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Ploeh.SemanticComparison.Fluent;

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

        [Theory, AutoAtomData]
        public void WithNameReturnsCorrectResult(
            [Modest]AtomAuthor sut,
            string newName)
        {
            AtomAuthor actual = sut.WithName(newName);

            var expected = actual.AsSource().OfLikeness<AtomAuthor>()
                .With(x => x.Name).EqualsWhen(
                    (s, d) => object.Equals(newName, s.Name));
            expected.ShouldEqual(actual);
        }
    }
}
