using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Azure.Storage.Blobs;
using Grean.AtomEventStore;
using Xunit;

namespace Grean.AtomEventStore.AzureBlobAsync.Tests
{
    public class AtomEventsOnAzureAsyncTests
    {
        [XmlRoot("test-event", Namespace = "urn:grean:azure-async-tests")]
        public class TestEvent
        {
            [XmlElement("number")]
            public int Number { get; set; }

            [XmlElement("text")]
            public string Text { get; set; }
        }

        private static BlobContainerClient CreateTransientContainer()
        {
            var connection =
                Environment.GetEnvironmentVariable(
                    IntegrationFactAttribute.ConnectionVariable);
            // Container names: lowercase alphanumerics, 3-63 chars.
            var name = "aestest" + Guid.NewGuid().ToString("N");
            return new BlobContainerClient(connection, name);
        }

        [IntegrationFact]
        public async Task RoundTripsEventsInOrderAcrossPages()
        {
            var container = CreateTransientContainer();
            await container.CreateIfNotExistsAsync();
            try
            {
                var storage = new AtomEventsOnAzureAsync(container);
                var serializer =
                    XmlContentSerializer.Scan(typeof(TestEvent).Assembly);
                var id = UuidIri.NewId();

                // pageSize = 2 forces a multi-page linked list on Azure.
                var writer =
                    new AsyncAtomEventObserver<TestEvent>(id, 2, storage, serializer);

                var expected = Enumerable.Range(1, 5)
                    .Select(i => new TestEvent { Number = i, Text = "event " + i })
                    .ToList();
                foreach (var e in expected)
                    await writer.AppendAsync(e);

                var reader = new AsyncFifoEvents<TestEvent>(id, storage, serializer);
                var actual = new List<TestEvent>();
                await foreach (var e in reader)
                    actual.Add(e);

                Assert.Equal(
                    expected.Select(e => e.Number),
                    actual.Select(e => e.Number));
                Assert.Equal(
                    expected.Select(e => e.Text),
                    actual.Select(e => e.Text));
            }
            finally
            {
                await container.DeleteIfExistsAsync();
            }
        }

        [IntegrationFact]
        public async Task EnumeratesWrittenStreamIds()
        {
            var container = CreateTransientContainer();
            await container.CreateIfNotExistsAsync();
            try
            {
                var storage = new AtomEventsOnAzureAsync(container);
                var serializer =
                    XmlContentSerializer.Scan(typeof(TestEvent).Assembly);

                var id1 = UuidIri.NewId();
                var id2 = UuidIri.NewId();
                await new AsyncAtomEventObserver<TestEvent>(id1, 25, storage, serializer)
                    .AppendAsync(new TestEvent { Number = 1, Text = "a" });
                await new AsyncAtomEventObserver<TestEvent>(id2, 25, storage, serializer)
                    .AppendAsync(new TestEvent { Number = 2, Text = "b" });

                var ids = new List<UuidIri>();
                await foreach (var sid in storage)
                    ids.Add(sid);

                Assert.Contains((UuidIri)id1, ids);
                Assert.Contains((UuidIri)id2, ids);
            }
            finally
            {
                await container.DeleteIfExistsAsync();
            }
        }
    }
}
