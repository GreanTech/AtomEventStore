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
    }
}
