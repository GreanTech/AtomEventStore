using Ploeh.AutoFixture.Idioms;
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
    public class AtomEventObserverTests
    {
        [Theory, AutoAtomData]
        public void PropertiesAreCorrectlyInitialized(
            ConstructorInitializedMemberAssertion assertion)
        {
            assertion.Verify(typeof(AtomEventObserver<TestEventX>));
        }

        [Theory, AutoAtomData]
        public void AppendAsyncCorrectlyStoresFeed(
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory storage,
            AtomEventObserver<XmlAttributedTestEventX> sut,
            XmlAttributedTestEventX expectedEvent)
        {
            var before = DateTimeOffset.Now;

            sut.AppendAsync(expectedEvent).Wait();

            var writtenFeeds = storage.Feeds.Select(ParseAtomFeed);
            var index = writtenFeeds.SingleOrDefault(f => f.Id == sut.Id);
            Assert.NotNull(index);
            var firstLink = index.Links.SingleOrDefault(l => l.IsFirstLink);
            Assert.NotNull(firstLink);
            Guid g;
            Assert.True(Guid.TryParse(firstLink.Href.ToString(), out g));
            var actual = writtenFeeds.SingleOrDefault(f => f.Id == (UuidIri)g);
            Assert.NotNull(actual);
            var expectedFeed = 
                new AtomFeedLikeness(before, actual.Id, expectedEvent);
            Assert.True(
                expectedFeed.Equals(actual),
                "Expected feed must match actual feed.");
        }

        private static AtomFeed ParseAtomFeed(string xml)
        {
            return AtomFeed.Parse(
                xml,
                new XmlContentSerializer(new TestEventTypeResolver()));
        }

        public class TestEventTypeResolver : ITypeResolver
        {
            public Type Resolve(string localName, string xmlNamespace)
            {
                switch (xmlNamespace)
                {
                    case "http://grean:rocks":
                        switch (localName)
                        {
                            case "test-event-x":
                                return typeof(XmlAttributedTestEventX);
                            case "test-event-y":
                                return typeof(XmlAttributedTestEventY);
                            default:
                                throw new ArgumentException("Unexpected local name.", "localName");
                        }
                    default:
                        throw new ArgumentException("Unexpected XML namespace.", "xmlNamespace");
                }
            }
        }
    }
}
