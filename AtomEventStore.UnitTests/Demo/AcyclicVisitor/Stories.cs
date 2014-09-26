using Ploeh.SemanticComparison;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Grean.AtomEventStore.UnitTests.Demo.AcyclicVisitor
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
                new DataContractContentSerializer(
                    new TypeResolutionTable(
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "user-created",
                            typeof(UserCreated)),
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "email-verified",
                            typeof(EmailVerified)),
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "email-changed",
                            typeof(EmailChanged))));
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
                new DataContractContentSerializer(
                    new TypeResolutionTable(
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "user-created",
                            typeof(UserCreated)),
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "email-verified",
                            typeof(EmailVerified)),
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "email-changed",
                            typeof(EmailChanged))));
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
                new DataContractContentSerializer(
                    new TypeResolutionTable(
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "user-created",
                            typeof(UserCreated)),
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "email-verified",
                            typeof(EmailVerified)),
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "email-changed",
                            typeof(EmailChanged))));
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

        [Fact]
        public void ReadMultipleEvents()
        {
            var eventStreamId =
                new Guid("A0E50259-7345-48F9-84B4-BEEB5CEC662C");
            var storage = new AtomEventsInMemory();
            var pageSize = 25;
            var serializer =
                new DataContractContentSerializer(
                    new TypeResolutionTable(
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "user-created",
                            typeof(UserCreated)),
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "email-verified",
                            typeof(EmailVerified)),
                        new TypeResolutionEntry(
                            "urn:grean:samples:user-on-boarding",
                            "email-changed",
                            typeof(EmailChanged))));
            var obs = new AtomEventObserver<object>(
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

            var events = new FifoEvents<object>(
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
