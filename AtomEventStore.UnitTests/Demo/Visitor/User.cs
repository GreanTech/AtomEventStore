using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests.Demo.Visitor
{
    public class User
    {
        private readonly Guid id;
        private readonly string name;
        private readonly string password;
        private readonly string email;
        private readonly bool emailVerified;

        public User(
            Guid id,
            string name,
            string password,
            string email,
            bool emailVerified)
        {
            this.id = id;
            this.name = name;
            this.password = password;
            this.email = email;
            this.emailVerified = emailVerified;
        }

        public static User Fold(IEnumerable<IUserEvent> events)
        {
            var result = events.Aggregate(
                new UserVisitor(),
                (v, e) => (UserVisitor)e.Accept(v));
            return result.Users.Single();
        }

        public Guid Id
        {
            get { return this.id; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public string Password
        {
            get { return this.password; }
        }

        public string Email
        {
            get { return this.email; }
        }

        public bool EmailVerified
        {
            get { return this.emailVerified; }
        }

        private class UserVisitor : IUserVisitor
        {
            private readonly IEnumerable<User> users;

            public UserVisitor(IEnumerable<User> users)
            {
                this.users = users;
            }

            public UserVisitor(params User[] users)
                : this(users.AsEnumerable())
            {
            }

            public IEnumerable<User> Users
            {
                get { return this.users; }
            }

            public IUserVisitor Visit(UserCreated @event)
            {
                var user = new User(
                    @event.UserId,
                    @event.UserName,
                    @event.Password,
                    @event.Email,
                    false);
                return new UserVisitor(this.users.Concat(new[] { user }));
            }

            public IUserVisitor Visit(EmailVerified @event)
            {
                var origUser = this.users.Single(u => u.id == @event.UserId);
                var newUser = new User(
                    origUser.Id,
                    origUser.Name,
                    origUser.Password,
                    origUser.Email,
                    true);
                var newUsers = this.users
                    .Where(u => u.id != newUser.id)
                    .Concat(new[] { newUser });
                return new UserVisitor(newUsers);
            }

            public IUserVisitor Visit(EmailChanged @event)
            {
                var origUser = this.users.Single(u => u.id == @event.UserId);
                var newUser = new User(
                    origUser.Id,
                    origUser.Name,
                    origUser.Password,
                    @event.NewEmail,
                    false);
                var newUsers = this.users
                    .Where(u => u.id != newUser.id)
                    .Concat(new[] { newUser });
                return new UserVisitor(newUsers);
            }
        }
    }
}
