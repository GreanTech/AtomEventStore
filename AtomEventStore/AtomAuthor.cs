using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public class AtomAuthor
    {
        private readonly string name;

        public AtomAuthor(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }

        public AtomAuthor WithName(string newName)
        {
            return new AtomAuthor(newName);
        }

        public override bool Equals(object obj)
        {
            var other = obj as AtomAuthor;
            if (other != null)
                return object.Equals(this.name, other.name);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }
    }
}
