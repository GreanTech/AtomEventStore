using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore
{
    internal class ContentSerializer
    {
        internal void Serialize(XmlWriter xmlWriter, object value)
        {
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
                xmlWriter.WriteValue(((DateTimeOffset)value).ToString("o"));
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
    }
}
