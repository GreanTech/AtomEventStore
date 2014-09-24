using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests.Demo.Visitor
{
    public class UserVisitor : IUserVisitor
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
            var origUser = this.users.Single(u => u.Id == @event.UserId);
            var newUser = new User(
                origUser.Id,
                origUser.Name,
                origUser.Password,
                origUser.Email,
                true);
            var newUsers = this.users
                .Where(u => u.Id != newUser.Id)
                .Concat(new[] { newUser });
            return new UserVisitor(newUsers);
        }

        public IUserVisitor Visit(EmailChanged @event)
        {
            var origUser = this.users.Single(u => u.Id == @event.UserId);
            var newUser = new User(
                origUser.Id,
                origUser.Name,
                origUser.Password,
                @event.NewEmail,
                false);
            var newUsers = this.users
                .Where(u => u.Id != newUser.Id)
                .Concat(new[] { newUser });
            return new UserVisitor(newUsers);
        }
    }
}
