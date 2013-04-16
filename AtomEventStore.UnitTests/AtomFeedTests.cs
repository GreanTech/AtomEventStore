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
    public class AtomFeedTests
    {
        [Theory, AutoAtomData]
        public void IdIsCorrect([Frozen]UuidIri expected, AtomFeed sut)
        {
            UuidIri actual = sut.Id;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void TitleIsCorrect([Frozen]string expected, AtomFeed sut)
        {
            string actual = sut.Title;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void UpdatedIsCorrect(
            [Frozen]DateTimeOffset expected,
            AtomFeed sut)
        {
            DateTimeOffset actual = sut.Updated;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void AuthorIsCorrect(
            [Frozen]AtomAuthor expected,
            AtomFeed sut)
        {
            AtomAuthor actual = sut.Author;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void EntriesIsCorrect(
            [Frozen]IEnumerable<AtomEntry> expected,
            AtomFeed sut)
        {
            IEnumerable<AtomEntry> actual = sut.Entries;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void LinksIsCorrect(
            [Frozen]IEnumerable<AtomLink> expected,
            AtomFeed sut)
        {
            IEnumerable<AtomLink> actual = sut.Links;
            Assert.Equal(expected, actual);
        }
    }
}
