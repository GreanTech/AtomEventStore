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
    public class SyndicationEventStreamTests
    {
        [Theory, AutoAtomData]
        public void AppendFirstEventSavesCorrectDocuments(
            [Frozen]string id,
            [Frozen]Mock<ISyndicationItemWriter> entryWriterMock,
            [Frozen]Mock<ISyndicationFeedWriter> headWriterMock,
            SyndicationEventStream<TestEvent> sut,
            TestEvent @event)
        {
            // Fixture setup
            var expectedEntry = new SyndicationItemBuilder()
                .WithXmlContent(@event)
                .Build()
                .ToResemblance();

            var expectedHead = new SyndicationFeedBuilder()
                .WithFeedId(id)
                .WithItem(expectedEntry
                    .Clone()
                    .ChangeLinkRelationShipTypes(from: "self", to: "via")
                    .ToResemblance())
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
            sut.Append(@event).Wait();

            // Verify outcome
            entryWriterMock.Verify();
            headWriterMock.Verify();
            Assert.True(
                sequence.IsOrdered,
                "Mocks were invoked out of expected order.");

            // Teardown
        }

        [Theory, AutoAtomData]
        public void AppendLaterEventSavesCorrectDocuments(
            [Frozen]string id,
            [Frozen]Mock<ISyndicationFeedReader> headReaderStub,
            [Frozen]Mock<ISyndicationItemWriter> entryWriterMock,
            [Frozen]Mock<ISyndicationFeedWriter> headWriterMock,
            SyndicationEventStream<TestEvent> sut,
            TestEvent previousEvent,
            TestEvent newEvent)
        {
            // Fixture setup
            var existingHead = new SyndicationFeedBuilder()
                .WithFeedId(id)
                .WithItem(new SyndicationItemBuilder()
                    .WithXmlContent(previousEvent)
                    .Build()
                    .ChangeLinkRelationShipTypes(from: "self", to: "via"))
                .Build();
            headReaderStub.Setup(r => r.ReadFeed(id)).Returns(existingHead);

            var existingHeadLink = existingHead.Items.Single()
                .Links.Single(l => l.RelationshipType == "via");
            var expectedEntry = new SyndicationItemBuilder()
                .WithXmlContent(newEvent)
                .WithLink(new SyndicationLink
                    {
                        RelationshipType = "previous",
                        Uri = existingHeadLink.Uri
                    })
                .Build()
                .ToResemblance();

            var expectedHead = new SyndicationFeedBuilder()
                .WithFeedId(id)
                .WithItem(expectedEntry
                    .Clone()
                    .ChangeLinkRelationShipTypes(from: "self", to: "via")
                    .ToResemblance())
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
            sut.Append(newEvent).Wait();

            // Verify outcome
            entryWriterMock.Verify();
            headWriterMock.Verify();
            Assert.True(
                sequence.IsOrdered,
                "Mocks were invoked out of expected order.");

            // Teardown
        }

        [Theory, AutoAtomData]
        public void IdIsCorrect([Frozen]string expected, SyndicationEventStream<TestEvent> sut)
        {
            string actual = sut.Id;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutIsEnumerable(SyndicationEventStream<TestEvent> sut)
        {
            Assert.IsAssignableFrom<IEnumerable<TestEvent>>(sut);
        }
    }
}
