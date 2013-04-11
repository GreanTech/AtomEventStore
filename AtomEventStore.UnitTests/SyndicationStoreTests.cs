using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Xunit;
using Moq;
using System.Xml;
using System.Xml.Linq;
using Xunit;
using System.ServiceModel.Syndication;

namespace Grean.AtomEventStore.UnitTests
{
    public class SyndicationStoreTests
    {
        [Theory, AutoAtomData]
        public void AppendFirstEventSavesCorrectDocuments(
            [Frozen]Mock<ISyndicationItemWriter> entryWriterMock,
            [Frozen]Mock<ISyndicationFeedWriter> headWriterMock,
            SyndicationStore sut,
            string id,
            TestEvent @event)
        {
            // Fixture setup
            var expectedEntry = new SyndicationItemBuilder()
                .WithXmlContent(@event)
                .Build()
                .ToResemblance();

            var expectedFeedItem = expectedEntry
                .Clone()
                .ChangeLinkRelationShipTypes(from: "self", to: "via")
                .ToResemblance();
            var expectedHead = new SyndicationFeedBuilder()
                .WithFeedId(id)
                .WithItem(expectedFeedItem)
                .Build()
                .ToResemblance();

            var sequence = new SpySequence();
            entryWriterMock
                .Setup(w => w.Create(expectedEntry))
                .InSequence(sequence)
                .Verifiable();
            headWriterMock
                .Setup(w => w.CreateOrUpdate(expectedHead))
                .InSequence(sequence)
                .Verifiable();

            // Exercise system
            sut.Append(id, @event).Wait();

            // Verify outcome
            entryWriterMock.Verify();
            headWriterMock.Verify();
            Assert.True(
                sequence.IsOrdered,
                "Mocks were invoked out of expected order.");

            // Teardown
        }
    }
}
