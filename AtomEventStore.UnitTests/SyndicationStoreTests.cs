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
            sut.Append(id, @event).Wait();

            var expectedEntry = new SyndicationItemBuilder()
                .WithXmlContent(@event)
                .Build()
                .ToResemblance();
            var expectedHead = new SyndicationFeedBuilder()
                .Build()
                .ToResemblance();
            entryWriterMock.Verify(w => w.Create(expectedEntry));
            headWriterMock.Verify(w => w.CreateOrUpdate(expectedHead));
        }
    }
}
