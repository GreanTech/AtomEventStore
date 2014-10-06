using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Grean.AtomEventStore.UnitTests.Demo.Visitor
{
    public class Stories
    {
        [Fact]
        public void WriteASingleEventSynchronously()
        {
            var eventStreamId =
                new Guid("A0E50259-7345-48F9-84B4-BEEB5CEC662C");
            /* Uses file storage to provide a documentation example of how to
             * do that. */
            var directory = 
                new DirectoryInfo(
                    Path.Combine(
                        Environment.CurrentDirectory,
                        Guid.NewGuid().ToString("N")));
            try
            {
                var storage = new AtomEventsInFiles(directory);
                var pageSize = 25;
                var serializer =
                    new DataContractContentSerializer(
                        new TypeResolutionTable(
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "user-created",
                                typeof(UserCreated)),
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "email-verified",
                                typeof(EmailVerified)),
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "email-changed",
                                typeof(EmailChanged))));
                IObserver<IUserEvent> obs = new AtomEventObserver<IUserEvent>(
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
            finally
            {
                directory.Delete(recursive: true);
            }
        }

        [Fact]
        public async Task WriteASingleEventAsynchronously()
        {
            var eventStreamId =
                new Guid("A0E50259-7345-48F9-84B4-BEEB5CEC662C");
            using (var storage = new AtomEventsInMemory())
            {
                var pageSize = 25;
                var serializer =
                    new DataContractContentSerializer(
                        new TypeResolutionTable(
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "user-created",
                                typeof(UserCreated)),
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "email-verified",
                                typeof(EmailVerified)),
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "email-changed",
                                typeof(EmailChanged))));
                var obs = new AtomEventObserver<IUserEvent>(
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
        }

        [Fact]
        public void ReadMultipleEvents()
        {
            var eventStreamId =
                new Guid("A0E50259-7345-48F9-84B4-BEEB5CEC662C");
            using (var storage = new AtomEventsInMemory())
            {
                var pageSize = 25;
                var serializer =
                    new DataContractContentSerializer(
                        new TypeResolutionTable(
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "user-created",
                                typeof(UserCreated)),
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "email-verified",
                                typeof(EmailVerified)),
                            new TypeResolutionEntry(
                                "urn:grean:samples:user-sign-up",
                                "email-changed",
                                typeof(EmailChanged))));
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
}
