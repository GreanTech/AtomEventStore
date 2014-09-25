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
            TypeResolutionEntry[] expected)
        {
            var sut = new TypeResolutionTable(expected);
            var actual = sut.Entries;
            Assert.True(expected.SequenceEqual(actual));
        }

        [Theory, AutoAtomData]
        public void EntriesIsCorrectWhenInitializedWithEnumerable(
            IEnumerable<TypeResolutionEntry> expected)
        {
            var sut = new TypeResolutionTable(expected);
            var actual = sut.Entries;
            Assert.True(expected.SequenceEqual(actual));
        }

        [Theory, AutoAtomData]
        public void ResolveReturnsCorrectResult(
            TypeResolutionEntry[] entries)
        {
            var entry = entries.PickRandom();
            var expected = entry.Resolution;
            var sut = new TypeResolutionTable(entries);

            var actual = sut.Resolve(entry.LocalName, entry.XmlNamespace);

            Assert.Equal(expected, actual);
        }
    }
}
