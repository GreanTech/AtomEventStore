using System;
using System.Collections;
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

        public XmlAtomContent(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            this.item = item;
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

            new ContentSerializer().Serialize(xmlWriter, this.item);

            xmlWriter.WriteEndElement();
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

            var elementName = node.Name.LocalName;
            var xmlNamespace = node.Name.NamespaceName;

            var dotNetNamespace = UnUrnify(xmlNamespace);
            var itemType = new XmlCasedName(elementName).ToTypeIn(dotNetNamespace);

            var ctor = GetMostModestConstructor(itemType);

            var namedArguments = (from p in ctor.GetParameters()
                                  let xpn = ContentSerializer.Xmlify(p.Name)
                                  join x in node.Elements() on xpn equals x.Name.LocalName
                                  select GetObjectFrom(x))
                                 .ToList();
            var paramsValues = (from x in node.Elements()
                                where !namedArguments.Select(a => a.Name).Contains(x.Name.LocalName)
                                select GetParamsObjectFrom(x))
                               .ToArray();

            if (itemType.IsGenericTypeDefinition)
            {
                var matchingArgumentType = GetMatchingArgument(namedArguments, paramsValues, itemType, ctor);

                itemType = itemType.MakeGenericType(matchingArgumentType);
                ctor = GetMostModestConstructor(itemType);
            }

            List<object> sortedArguments = new List<object>();
            foreach (var p in ctor.GetParameters())
            {
                if (Attribute.IsDefined(p, typeof(ParamArrayAttribute)))
                {
                    var elementType = p.ParameterType.GetElementType();
                    var arr = Array.CreateInstance(elementType, paramsValues.Length);
                    paramsValues.Select(a => a.Value).ToArray().CopyTo(arr, 0);
                    sortedArguments.Add(arr);
                }
                else
                {
                    var argValue = namedArguments.Single(kvp => UnXmlify(kvp.Name).Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                    sortedArguments.Add(ChangeType(argValue.Value, p.ParameterType));
                }
            }

            var item = ctor.Invoke(sortedArguments.ToArray());
            return item;
        }

        private class Argument
        {
            public readonly string Name;
            public readonly object Value;

            public Argument(string name, object value)
            {
                this.Name = name;
                this.Value = value;
            }
        }

        private static ConstructorInfo GetMostModestConstructor(Type itemType)
        {
            return (from c in itemType.GetConstructors()
                    let args = c.GetParameters()
                    orderby args.Length
                    select c).First();
        }

        private static Argument GetObjectFrom(XElement node)
        {
            if (!node.HasElements)
                return new Argument(node.Name.LocalName, node.Value);

            var value = ReadFrom(node.Elements().Single());
            return new Argument(node.Name.LocalName, value);
        }

        private static Argument GetParamsObjectFrom(XElement node)
        {
            var value = ReadFrom(node);
            return new Argument(node.Name.LocalName, value);
        }

        private static Type GetMatchingArgument(List<Argument> namedArguments, Argument[] paramsValues, Type itemType, ConstructorInfo ctor)
        {
            return GetDirectMatchingArgument(namedArguments, itemType, ctor).Concat(
                GetArrayMatchingArgument(paramsValues))
                .First();
        }

        private static IEnumerable<Type> GetDirectMatchingArgument(List<Argument> arguments, Type itemType, ConstructorInfo ctor)
        {
            var openGenericType = itemType.GetGenericArguments().Single();
            var genericCtorParamter = ctor.GetParameters()
                .SingleOrDefault(p => p.ParameterType == openGenericType);
            if (genericCtorParamter == null)
                yield break;

            var matchingArgument = arguments
                .Single(kvp => UnXmlify(kvp.Name).Equals(genericCtorParamter.Name, StringComparison.OrdinalIgnoreCase))
                .Value;
            yield return matchingArgument.GetType();
        }

        private static IEnumerable<Type> GetArrayMatchingArgument(IEnumerable<Argument> paramsValues)
        {
            var interfaces = from a in paramsValues
                             select GetBaseTypes(a.Value.GetType());

            var commonInterfaces = interfaces.Aggregate((x, y) => x.Intersect(y).ToArray());
            var commonInterface = commonInterfaces.FirstOrDefault();
            if (commonInterface == null)
                yield break;

            yield return commonInterface;
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            return new[] { type }.Concat(type.GetInterfaces());
        }

        private static object ChangeType(object value, Type type)
        {
            if (type == typeof(Guid))
                return (Guid)UuidIri.Parse(value.ToString());
            if (type == typeof(Uri))
                return new Uri(value.ToString());
            if (type == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(value.ToString(), CultureInfo.InvariantCulture);

            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        private static string UnUrnify(string text)
        {
            return string.Join(".", text.Replace("urn:", "").Split(':').Select(UnXmlify));
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
                    ResolveAssembly,
                    ResolveType);

                return type;
            }

            private string GetTypeName()
            {
                return this.ToPascalCase();
            }

            private static Assembly ResolveAssembly(AssemblyName assemblyName)
            {
                Assembly foundAssembly = null;
                var nameCandidate = (AssemblyName)assemblyName.Clone();
                while (foundAssembly == null)
                {
                    try
                    {
                        foundAssembly = Assembly.Load(nameCandidate);
                    }
                    catch (FileNotFoundException)
                    {
                        var dotIndex = nameCandidate.Name.LastIndexOf('.');
                        if (dotIndex < 0)
                            throw;
                        nameCandidate.Name = nameCandidate.Name.Substring(0, dotIndex);
                    }
                }

                return foundAssembly;
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
                        (t.Name == typeName && !IsStatic(t)) ||
                        t.Name == typeName + "`1")
                    .Single();
            }

            private static bool IsStatic(Type type)
            {
                return type.IsAbstract && type.IsSealed;
            }

            public override string ToString()
            {
                return this.value;
            }
        }
    }
}
