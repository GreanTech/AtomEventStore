using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit;
using Xunit.Extensions;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventStorageTests
    {
        [Theory, AutoAtomData]
        public void CreateNewFeedReturnsCorrectResult(
            Guid id,
            IContentSerializer dummySerializer)
        {
            var href = new Uri(id.ToString(), UriKind.Relative);
            var before = DateTimeOffset.Now;

            XmlReader actual = AtomEventStorage.CreateNewFeed(href);

            var actualFeed = AtomFeed.ReadFrom(actual, dummySerializer);
            Assert.Equal<Guid>(id, actualFeed.Id);
            Assert.True(before <= actualFeed.Updated);
            Assert.True(actualFeed.Updated <= DateTimeOffset.Now);
            Assert.Empty(actualFeed.Entries);
            Assert.Contains(AtomLink.CreateSelfLink(href), actualFeed.Links);
        }

        [Theory, AutoAtomData]
        public void CreateNewFeedForSegmentedUrlReturnsCorrectResult(
            Guid id,
            Guid segment,
            IContentSerializer dummySerializer)
        {
            var href = new Uri(
                segment.ToString() + "/" + id.ToString(),
                UriKind.Relative);
            var before = DateTimeOffset.Now;

            XmlReader actual = AtomEventStorage.CreateNewFeed(href);

            var actualFeed = AtomFeed.ReadFrom(actual, dummySerializer);
            Assert.Equal<Guid>(id, actualFeed.Id);
            Assert.True(before <= actualFeed.Updated);
            Assert.True(actualFeed.Updated <= DateTimeOffset.Now);
            Assert.Empty(actualFeed.Entries);
            Assert.Contains(AtomLink.CreateSelfLink(href), actualFeed.Links);
        }
    }
}
