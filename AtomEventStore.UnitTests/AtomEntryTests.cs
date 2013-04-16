using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEntryTests
    {
        [Theory, AutoAtomData]
        public void IdIsCorrect([Frozen]UuidIri expected, AtomEntry sut)
        {
            UuidIri actual = sut.Id;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void TitleIsCorrect([Frozen]string expected, AtomEntry sut)
        {
            string actual = sut.Title;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void PublishedReturnsCorrectResult(
            [Frozen]DateTimeOffset expected,
            AtomEntry sut)
        {
            DateTimeOffset actual = sut.Published;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void UpdatedReturnsCorrectResult(
            [Frozen]DateTimeOffset expected,
            AtomEntry sut)
        {
            DateTimeOffset actual = sut.Updated;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void PublishedAndUpdatedCanBeDifferent(AtomEntry sut)
        {
            Assert.NotEqual(sut.Published, sut.Updated);
        }

        [Theory, AutoAtomData]
        public void AuthorIsCorrect([Frozen]AtomAuthor expected, AtomEntry sut)
        {
            AtomAuthor actual = sut.Author;
            Assert.Equal(expected, actual);
        }
    }
}
