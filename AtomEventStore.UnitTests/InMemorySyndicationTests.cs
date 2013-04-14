using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using System.ServiceModel.Syndication;

namespace Grean.AtomEventStore.UnitTests
{
    public class InMemorySyndicationTests
    {
        [Theory, AutoAtomData]
        public void SutIsSyndicationFeedReader(InMemorySyndication sut)
        {
            Assert.IsAssignableFrom<ISyndicationFeedReader>(sut);
        }

        [Theory, AutoAtomData]
        public void ReadFeedFromEmptySutReturnsCorrectResult(
            InMemorySyndication sut,
            string id)
        {
            var actual = sut.Read(id);
            Assert.Empty(actual.Items);
        }

        [Theory, AutoAtomData]
        public void SutIsSyndicationFeedWriter(InMemorySyndication sut)
        {
            Assert.IsAssignableFrom<ISyndicationFeedWriter>(sut);
        }

        [Theory, AutoAtomData]
        public void CreateOrUpdateStoresFeedForReading(
            InMemorySyndication sut,
            SyndicationFeed expected,
            string id)
        {
            expected.Links.AddId(id);

            sut.CreateOrUpdate(expected);
            var actual = sut.Read(id);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void ReadReturnsCorrectFeed(
            InMemorySyndication sut,
            SyndicationFeed expected,
            string id,
            SyndicationFeed otherFeed,
            string otherId)
        {
            expected.Links.AddId(id);
            sut.CreateOrUpdate(expected);
            otherFeed.Links.AddId(otherId);
            sut.CreateOrUpdate(otherFeed);

            var actual = sut.Read(id);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutIsSyndicationItemWriter(InMemorySyndication sut)
        {
            Assert.IsAssignableFrom<ISyndicationItemWriter>(sut);
        }

        [Theory, AutoAtomData]
        public void CreateSameItemTwiceThrows(
            InMemorySyndication sut,
            SyndicationItem item)
        {
            sut.Create(item);

            Assert.Throws<ArgumentException>(() => sut.Create(item));
        }
    }
}
