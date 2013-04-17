using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

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

        public void WriteTo(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("author", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteElementString("name", this.name);
            xmlWriter.WriteEndElement();
        }
    }
}
