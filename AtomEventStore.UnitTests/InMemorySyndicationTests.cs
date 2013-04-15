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
        [Theory, AutoAtomMoqData]
        public void SutIsSyndicationFeedReader(InMemorySyndication sut)
        {
            Assert.IsAssignableFrom<ISyndicationFeedReader>(sut);
        }

        [Theory, AutoAtomMoqData]
        public void ReadFeedFromEmptySutReturnsCorrectResult(
            InMemorySyndication sut,
            string id)
        {
            var actual = sut.ReadFeed(id);
            Assert.Empty(actual.Items);
        }

        [Theory, AutoAtomMoqData]
        public void SutIsSyndicationFeedWriter(InMemorySyndication sut)
        {
            Assert.IsAssignableFrom<ISyndicationFeedWriter>(sut);
        }

        [Theory, AutoAtomMoqData]
        public void CreateOrUpdateStoresFeedForReading(
            InMemorySyndication sut,
            SyndicationFeed expected,
            string id)
        {
            expected.Links.AddId(id);

            sut.CreateOrUpdate(expected);
            var actual = sut.ReadFeed(id);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomMoqData]
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

            var actual = sut.ReadFeed(id);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomMoqData]
        public void SutIsSyndicationItemWriter(InMemorySyndication sut)
        {
            Assert.IsAssignableFrom<ISyndicationItemWriter>(sut);
        }

        [Theory, AutoAtomMoqData]
        public void CreateSameItemTwiceThrows(
            InMemorySyndication sut,
            SyndicationItem item,
            string id)
        {
            item.Links.AddId(id);
            sut.Create(item);

            Assert.Throws<ArgumentException>(() => sut.Create(item));
        }

        [Theory, AutoAtomMoqData]
        public void CreateTwoDifferentItemsDoesNotThrow(
            InMemorySyndication sut,
            SyndicationItem itemX,
            string idX,
            SyndicationItem itemY,
            string idY)
        {
            itemX.Links.AddId(idX);
            sut.Create(itemX);

            itemY.Links.AddId(idY);
            Assert.DoesNotThrow(() => sut.Create(itemY));
        }

        [Theory, AutoAtomMoqData]
        public void ReadItemCanRetrieveCorrectItem(
            InMemorySyndication sut,
            SyndicationItem expected,
            string idOfExpected,
            SyndicationItem other,
            string idOfOther)
        {
            // Fixture setup
            expected.Links.AddId(idOfExpected);
            sut.Create(expected);

            other.Links.AddId(idOfOther);
            sut.Create(other);

            // Exercise system
            SyndicationItem actual = sut.ReadItem(idOfExpected);

            // Verify outcome
            Assert.Equal(expected, actual);
            // Teardown
        }

        [Theory, AutoAtomMoqData]
        public void SutIsSyndicationItemReader(InMemorySyndication sut)
        {
            Assert.IsAssignableFrom<ISyndicationItemReader>(sut);
        }
    }
}
