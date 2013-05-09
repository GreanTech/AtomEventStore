﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Grean.AtomEventStore
{
    public class XmlAtomContent : IXmlWritable
    {
        private readonly object item;
        private readonly Type itemType;
        private readonly string itemXmlNamespace;

        public XmlAtomContent(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            this.item = item;
            this.itemType = item.GetType();
            this.itemXmlNamespace = Urnify(this.itemType.Namespace);
        }

        public object Item
        {
            get { return this.item; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "While the documentation of this CA warning mostly states that you can suppress this warning for already shipped code, as it would be a breaking change to address it, I'm taking the reverse position: making it static now would mean that it'd be a breaking change to make it an instance method later. All these 'With' methods are, in their nature, instance methods. The only reason the 'this' keyword isn't used here is because there's only a single field on the class, but this may change in the future.")]
        public XmlAtomContent WithItem(object newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException("newItem");

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
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");

            xmlWriter.WriteStartElement("content", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteAttributeString("type", "application/xml");

            this.WriteComplexObject(xmlWriter, this.item);

            xmlWriter.WriteEndElement();
        }

        private void WriteComplexObject(XmlWriter xmlWriter, object value)
        {
            var type = value.GetType();

            xmlWriter.WriteStartElement(Xmlify(type), this.itemXmlNamespace);
            foreach (var p in type.GetProperties())
            {
                var localName = Xmlify(p.Name);
                var v = p.GetValue(value);

                xmlWriter.WriteStartElement(localName);
                this.WriteValue(xmlWriter, v);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
        }

        private void WriteValue(XmlWriter xmlWriter, object value)
        {
            if (value is Guid)
            {
                xmlWriter.WriteValue(((UuidIri)((Guid)value)).ToString());
                return;
            }

            if (value is Uri)
            {
                xmlWriter.WriteValue(value.ToString());
                return;
            }

            if (IsCustomType(value.GetType()))
            {
                this.WriteComplexObject(xmlWriter, value);
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

        public static XmlAtomContent Parse(string xml)
        {
            var sr = new StringReader(xml);
            try
            {
                using (var r = XmlReader.Create(sr))
                {
                    sr = null;
                    return XmlAtomContent.ReadFrom(r);
                }
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
            }
        }

        public static XmlAtomContent ReadFrom(XmlReader xmlReader)
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");

            xmlReader.MoveToContent();
            var x = (XElement)XElement.ReadFrom(xmlReader);
            var item = ReadFrom(x.Elements().Single());
            return new XmlAtomContent(item);
        }

        private static object ReadFrom(XElement node)
        {
            if (!node.HasElements)
                return node.Value;

            var arguments = node.Elements()
                .Select(GetObjectFrom)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var elementName = node.Name.LocalName;
            var xmlNamespace = node.Name.NamespaceName;

            var dotNetNamespace = UnUrnify(xmlNamespace);
            var itemType = new XmlCasedName(elementName).ToTypeIn(dotNetNamespace);

            var ctor = GetMostModestConstructor(itemType);

            if (itemType.IsGenericTypeDefinition)
            {
                var openGenericType = itemType.GetGenericArguments().Single();
                var genericCtorParamter = ctor.GetParameters()
                    .Single(p => p.ParameterType == openGenericType);
                var matchingArgument = arguments
                    .Single(kvp => UnXmlify(kvp.Key.LocalName).Equals(genericCtorParamter.Name, StringComparison.OrdinalIgnoreCase))
                    .Value;

                itemType = itemType.MakeGenericType(matchingArgument.GetType());
                ctor = GetMostModestConstructor(itemType);
            }

            List<object> sortedArguments = new List<object>();
            foreach (var p in ctor.GetParameters())
            {
                var argValue = arguments.Single(kvp => UnXmlify(kvp.Key.LocalName).Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                sortedArguments.Add(ChangeType(argValue.Value, p.ParameterType));
            }

            var item = ctor.Invoke(sortedArguments.ToArray());
            return item;
        }

        private static ConstructorInfo GetMostModestConstructor(Type itemType)
        {
            return (from c in itemType.GetConstructors()
                    let args = c.GetParameters()
                    orderby args.Length
                    select c).First();
        }

        private static KeyValuePair<XName, object> GetObjectFrom(XElement node)
        {
            if (!node.HasElements)
                return new KeyValuePair<XName, object>(node.Name, node.Value);

            var value = ReadFrom(node.Elements().Single());
            return new KeyValuePair<XName, object>(node.Name, value);
        }

        private static object ChangeType(object value, Type type)
        {
            if (type == typeof(Guid))
                return (Guid)UuidIri.Parse(value.ToString());
            if (type == typeof(Uri))
                return new Uri(value.ToString());

            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
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
            return XmlCasedName.FromType(type).ToString();
        }

        private static string Xmlify(string text)
        {
            return XmlCasedName.FromText(text).ToString();
        }

        private static string UnXmlify(string text)
        {
            return new XmlCasedName(text).ToPascalCase();
        }

        private class XmlCasedName
        {
            private readonly string value;

            public XmlCasedName(string value)
            {
                this.value = value;
            }

            public static XmlCasedName FromType(Type type)
            {
                if (type.IsGenericType)
                {
                    var nonGenericName = type.Name.Replace("`1", "");
                    return XmlCasedName.FromText(nonGenericName);
                }

                return XmlCasedName.FromText(type.Name);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "This rule concerns itself with certain international characters that can't properly make a round-trip if lower-cased. However, this entire algorihtm (explicitly) uses the invariant culture, where this shouldn't be a problem. In any case, the reason for lower-casing isn't to perform normalization, but explicitly in order to convert to lower case - that's the desired outcome.")]
            public static XmlCasedName FromText(string text)
            {
                return new XmlCasedName(text
                    .Take(1).Select(c => Char.ToLower(c, CultureInfo.InvariantCulture))
                    .Concat(text.Skip(1))
                    .Aggregate("", (s, c) => Char.IsUpper(c) ? s + "-" + c : s + c)
                    .ToLower(CultureInfo.InvariantCulture));
            }

            public static XmlCasedName operator +(XmlCasedName xmlName, string text)
            {
                return xmlName + XmlCasedName.FromText(text);
            }

            public static XmlCasedName operator +(XmlCasedName a, XmlCasedName b)
            {
                return new XmlCasedName(a.value + "-" + b.value);
            }

            public string ToPascalCase()
            {
                return this.value.Split('-')
                    .Select(s => new string(s.Take(1).Select(Char.ToUpper).Concat(s.Skip(1)).ToArray()))
                    .Aggregate((x, y) => x + y);
            }

            public Type ToTypeIn(string dotNetNamespace)
            {
                var typeName = this.GetTypeName();

                var type = Type.GetType(
                    typeName + ", " + dotNetNamespace,
                    Assembly.Load,
                    ResolveType);

                return type;
            }

            private string GetTypeName()
            {
                return this.ToPascalCase();
            }

            private static Type ResolveType(
                Assembly assembly,
                string typeName,
                bool ignoreCase)
            {
                if (assembly == null)
                    return null;
                return assembly.GetExportedTypes()
                    .Where(t =>
                        (t.Name == typeName && !t.IsAbstract && !t.IsSealed) ||
                        t.Name == typeName + "`1")
                    .Single();
            }

            public override string ToString()
            {
                return this.value;
            }
        }
    }
}
