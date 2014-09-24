using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Grean.AtomEventStore.UnitTests.Demo.Visitor
{
    public class Stories
    {
        [Fact]
        public void ReadMultipleEvents()
        {
            var eventStreamId =
                new Guid("A0E50259-7345-48F9-84B4-BEEB5CEC662C");
            var storage = new AtomEventsInMemory();
            var pageSize = 25;
            var serializer =
                new DataContractContentSerializer(new UserTypeResolver());
            var obs = new AtomEventObserver<IUserEvent>(
                eventStreamId,
                pageSize,
                storage,
                serializer);
            obs.OnNext(new UserCreated
            {
                UserId = eventStreamId,
                UserName = "ploeh",
                Password = "12345",
                Email = "ploeh@fnaah.dk"
            });
            obs.OnNext(new EmailVerified
            {
                UserId = eventStreamId,
                Email = "ploeh@fnaah.dk"
            });
            obs.OnNext(new EmailChanged
            {
                UserId = eventStreamId,
                NewEmail = "fnaah@ploeh.dk"
            });

            var events = new FifoEvents<IUserEvent>(
                eventStreamId, // a Guid
                storage,       // an IAtomEventStorage object
                serializer);   // an IContentSerializer object
            var user = User.Fold(events);

            Assert.Equal(eventStreamId, user.Id);
            Assert.Equal("ploeh", user.Name);
            Assert.Equal("12345", user.Password);
            Assert.Equal("fnaah@ploeh.dk", user.Email);
            Assert.False(user.EmailVerified);
        }
    }
}
