using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Grean.AtomEventStore
{
    public class ConventionBasedSerializerOfComplexImmutableClasses : IContentSerializer
    {
        public void Serialize(XmlWriter xmlWriter, object value)
        {
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");
            if (value == null)
                throw new ArgumentNullException("value");

            WriteComplexObject(xmlWriter, value);
        }

        private static void WriteComplexObject(XmlWriter xmlWriter, object value)
        {
            var type = value.GetType();
            var xmlNamespace = Urnify(type.Namespace);

            xmlWriter.WriteStartElement(Xmlify(type), xmlNamespace);
            foreach (var p in type.GetProperties())
            {
                var localName = Xmlify(p.Name);
                var v = p.GetValue(value);

                xmlWriter.WriteStartElement(localName);
                WriteValue(xmlWriter, v);
                xmlWriter.WriteEndElement();
            }

            var sequence = value as IEnumerable;
            if (sequence != null)
            {
                foreach (var x in sequence)
                    WriteComplexObject(xmlWriter, x);
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

            if (value is Uri)
            {
                xmlWriter.WriteValue(value.ToString());
                return;
            }
            if (value is DateTimeOffset)
            {
                xmlWriter.WriteValue(
                    ((DateTimeOffset)value).ToString(
                        "o",
                        CultureInfo.InvariantCulture));
                return;
            }

            if (IsCustomType(value.GetType()))
            {
                WriteComplexObject(xmlWriter, value);
                return;
            }

            xmlWriter.WriteValue(value);
        }

        private static string Urnify(string text)
        {
            return "urn:" + string.Join(":", text.Split('.').Select(Xmlify));
        }

        private static string Xmlify(Type type)
        {
            return XmlCasedName.FromType(type).ToString();
        }

        internal static string Xmlify(string text)
        {
            return XmlCasedName.FromText(text).ToString();
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

        public XmlAtomContent Deserialize(XmlReader xmlReader)
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");

            var x = (XElement)XElement.ReadFrom(xmlReader);
            var item = ReadFrom(x);
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
                                  let xpn = ConventionBasedSerializerOfComplexImmutableClasses.Xmlify(p.Name)
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

        private static string UnUrnify(string text)
        {
            return string.Join(".", text.Replace("urn:", "").Split(':').Select(UnXmlify));
        }

        private static string UnXmlify(string text)
        {
            return new XmlCasedName(text).ToPascalCase();
        }

        private static ConstructorInfo GetMostModestConstructor(Type itemType)
        {
            return (from c in itemType.GetConstructors()
                    let args = c.GetParameters()
                    orderby args.Length
                    select c).First();
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

        private static Argument GetParamsObjectFrom(XElement node)
        {
            var value = ReadFrom(node);
            return new Argument(node.Name.LocalName, value);
        }

        private static Argument GetObjectFrom(XElement node)
        {
            if (!node.HasElements)
                return new Argument(node.Name.LocalName, node.Value);

            var value = ReadFrom(node.Elements().Single());
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
    }
}
