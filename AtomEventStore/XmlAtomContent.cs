using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Grean.AtomEventStore
{
    public class XmlAtomContent : IXmlWritable
    {
        private readonly object item;
        private readonly Type itemType;
        private readonly string itemXmlElement;
        private readonly string itemXmlNamespace;

        public XmlAtomContent(object item)
        {
            this.item = item;
            this.itemType = item.GetType();
            this.itemXmlElement = Xmlify(this.itemType);
            this.itemXmlNamespace = Urnify(this.itemType.Namespace);
        }

        public object Item
        {
            get { return this.item; }
        }

        public XmlAtomContent WithItem(object newItem)
        {
            return new XmlAtomContent(newItem);
        }

        public override bool Equals(object obj)
        {
            var other = obj as XmlAtomContent;
            if (other != null)
                return object.Equals(this.item, other.item);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.item.GetHashCode();
        }

        public void WriteTo(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("content", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteAttributeString("type", "application/xml");

            WriteValue(xmlWriter, this.item, this.itemXmlNamespace);

            xmlWriter.WriteEndElement();
        }

        private static void WriteValue(
            XmlWriter xmlWriter,
            object value,
            string xmlNamespace)
        {
            var type = value.GetType();

            xmlWriter.WriteStartElement(Xmlify(type), xmlNamespace);
            foreach (var p in type.GetProperties())
            {
                var localName = Xmlify(p.Name);
                var v = p.GetValue(value);

                xmlWriter.WriteStartElement(localName);
                WriteValue(xmlWriter, v);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
        }

        private static void WriteValue(XmlWriter xmlWriter, object value)
        {
            if (value is Guid)
            {
                xmlWriter.WriteValue(((UuidIri)((Guid)value)).ToString());
                return;
            }

            if (IsCustomType(value.GetType()))
            {
                WriteValue(xmlWriter, value, null);
                return;
            }

            xmlWriter.WriteValue(value);
        }

        private static bool IsCustomType(Type type)
        {
            return type != typeof(bool)
                && type != typeof(DateTime)
                && type != typeof(DateTimeOffset)
                && type != typeof(decimal)
                && type != typeof(double)
                && type != typeof(float)
                && type != typeof(int)
                && type != typeof(long)
                && type != typeof(string);
        }

        public static XmlAtomContent ReadFrom(XmlReader xmlReader)
        {
            var navigator = new XPathDocument(xmlReader).CreateNavigator();

            var resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            var contentElement = navigator.Select("/atom:content/*", resolver).Cast<XPathNavigator>()
                .Single();

            var elementName = contentElement.LocalName;
            var xmlNamespace = contentElement.NamespaceURI;

            resolver.AddNamespace("xn", xmlNamespace);

            var typeName = UnXmlify(elementName);
            var dotNetNamespace = UnUrnify(xmlNamespace);
            if (typeName.EndsWith("]]"))
                typeName = typeName.Replace("]]", ", " + dotNetNamespace + "]]");

            var itemType = Type.GetType(
                typeName + ", " + dotNetNamespace,
                an => Assembly.Load(an),
                ResolveType);
            var ctor = (from c in itemType.GetConstructors()
                        let args = c.GetParameters()
                        orderby args.Length
                        select c).First();            

            var arguments = ctor.GetParameters()
                .Select(p => GetValueFrom(navigator.Select("//xn:" + elementName + "/xn:" + Xmlify(p.Name), resolver).Cast<XPathNavigator>().Single(), resolver, p.ParameterType))
                .ToArray();
            var item = ctor.Invoke(arguments);

            return new XmlAtomContent(item);
        }

        private static object GetValueFrom(XPathNavigator navigator, IXmlNamespaceResolver resolver, Type type)
        {
            if (type == typeof(Guid))
                return (Guid)UuidIri.Parse(navigator.Value);

            if (IsCustomType(type))
            {
                var ctor = (from c in type.GetConstructors()
                            let args = c.GetParameters()
                            orderby args.Length
                            select c).First();

                var arguments = ctor.GetParameters()
                    .Select(p => GetValueFrom(navigator.Select("//xn:" + Xmlify(type) + "/xn:" + Xmlify(p.Name), resolver).Cast<XPathNavigator>().Single(), resolver, p.ParameterType))
                    .ToArray();
                var item = ctor.Invoke(arguments);

                return item;
            }

            return navigator.ValueAs(type);
        }

        private static Type ResolveType(
            Assembly assembly, 
            string typeName, 
            bool ignoreCase)
        {
            if (assembly == null)
                return null;
            return assembly.GetExportedTypes()
                .Where(t => t.Name == typeName)
                .Single();
        }

        private static string Urnify(string text)
        {
            return "urn:" + string.Join(":", text.Split('.').Select(Xmlify));
        }

        private static string UnUrnify(string text)
        {
            return string.Join(".", text.Replace("urn:", "").Split(':').Select(UnXmlify));
        }

        private static string Xmlify(Type type)
        {
            if (type.IsGenericType)
            {
                var nonGenericName = type.Name.Replace("`1", "");
                var gt = type.GetGenericArguments().Single();
                return Xmlify(nonGenericName) + "-of-" + Xmlify(gt);
            }

            return Xmlify(type.Name);
        }

        private static string Xmlify(string text)
        {
            return text
                .Take(1).Select(Char.ToLower).Concat(text.Skip(1))
                .Aggregate("", (s, c) => Char.IsUpper(c) ? s + "-" + c : s + c)
                .ToLower();
        }

        private static string UnXmlify(string text)
        {
            var index = text.IndexOf("-of-");
            if (index > 0)
            {
                var typeName = UnUrnify(text.Substring(0, index) + "`1");
                var genericName = UnUrnify(text.Substring(index + 4));
                return typeName + "[[" + genericName + "]]";
            }

            return text.Split('-')
                .Select(s => new string(s.Take(1).Select(Char.ToUpper).Concat(s.Skip(1)).ToArray()))
                .Aggregate((x, y) => x + y);
        }

        public static XmlAtomContent Parse(string xml)
        {
            using (var sr = new StringReader(xml))
            using (var r = XmlReader.Create(sr))
                return XmlAtomContent.ReadFrom(r);
        }
    }
}
