using Ploeh.SemanticComparison;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Grean.AtomEventStore.UnitTests.Demo.UserOnBoarding
{
    public class Stories
    {
        [Fact]
        public void WriteASingleEventSynchronously()
        {
            var eventStreamId = 
                new Guid("A0E50259-7345-48F9-84B4-BEEB5CEC662C");
            var storage = new AtomEventsInMemory();
            var pageSize = 25;
            var serializer = 
                new DataContractContentSerializer(new UserTypeResolver());
            IObserver<object> obs = new AtomEventObserver<object>(
                eventStreamId, // a Guid
                pageSize,      // an Int32
                storage,       // an IAtomEventStorage object
                serializer);   // an IContentSerializer object

            var userCreated = new UserCreated
            {
                UserId = eventStreamId,
                UserName = "ploeh",
                Password = "12345",
                Email = "ploeh@fnaah.com"
            };
            obs.OnNext(userCreated);

            Assert.NotEmpty(storage);
        }

        [Fact]
        public async Task WriteASingleEventAsynchronously()
        {
            var eventStreamId =
                new Guid("A0E50259-7345-48F9-84B4-BEEB5CEC662C");
            var storage = new AtomEventsInMemory();
            var pageSize = 25;
            var serializer =
                new DataContractContentSerializer(new UserTypeResolver());
            var obs = new AtomEventObserver<object>(
                eventStreamId, // a Guid
                pageSize,      // an Int32
                storage,       // an IAtomEventStorage object
                serializer);   // an IContentSerializer object

            var userCreated = new UserCreated
            {
                UserId = eventStreamId,
                UserName = "ploeh",
                Password = "12345",
                Email = "ploeh@fnaah.com"
            };
            await obs.AppendAsync(userCreated);

            Assert.NotEmpty(storage);
        }

        [Fact]
        public void ReadASingleEvent()
        {
            var eventStreamId =
                new Guid("A0E50259-7345-48F9-84B4-BEEB5CEC662C");
            var storage = new AtomEventsInMemory();
            var pageSize = 25;
            var serializer =
                new DataContractContentSerializer(new UserTypeResolver());
            var obs = new AtomEventObserver<object>(
                eventStreamId,
                pageSize, 
                storage,
                serializer);
            var userCreated = new UserCreated
            {
                UserId = eventStreamId,
                UserName = "ploeh",
                Password = "12345",
                Email = "ploeh@fnaah.com"
            };
            obs.OnNext(userCreated);

            IEnumerable<object> events = new FifoEvents<object>(
                eventStreamId, // a Guid
                storage,       // an IAtomEventStorage object
                serializer);   // an IContentSerializer object
            var firstEvent = events.First();

            var uc = Assert.IsAssignableFrom<UserCreated>(firstEvent);
            Assert.Equal(userCreated, uc, new SemanticComparer<UserCreated>());
        }
    }
}
