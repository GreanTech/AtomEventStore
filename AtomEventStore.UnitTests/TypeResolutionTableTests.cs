using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
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

        [Theory, AutoAtomData]
        public void EntriesIsCorrectWhenInitializedWithArray(
            [Frozen]TypeResolutionEntry[] expected,
            [FavorArrays]TypeResolutionTable sut)
        {
            var actual = sut.Entries;
            Assert.True(expected.SequenceEqual(actual));
        }

        [Theory, AutoAtomData]
        public void EntriesIsCorrectWhenInitializedWithEnumerable(
            [Frozen]IReadOnlyCollection<TypeResolutionEntry> expected,
            [FavorEnumerables]TypeResolutionTable sut)
        {
            var actual = sut.Entries;
            Assert.True(expected.SequenceEqual(actual));
        }

        [Theory, AutoAtomData]
        public void ResolveReturnsCorrectResult(
            [FavorArrays]TypeResolutionTable sut)
        {
            var entry = sut.Entries.ToArray().PickRandom();
            var expected = entry.ResolvedType;

            var actual = sut.Resolve(entry.LocalName, entry.XmlNamespace);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void ResolveThrowsWhenInputCanNotBeMappedToProperType(
            TypeResolutionEntry notMapped,
            TypeResolutionEntry[] entries)
        {
            var sut = new TypeResolutionTable(entries);
            Assert.Throws<ArgumentException>(() =>
                sut.Resolve(notMapped.LocalName, notMapped.XmlNamespace));
        }
    }
}
