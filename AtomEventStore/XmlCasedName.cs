using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    internal class XmlCasedName
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
