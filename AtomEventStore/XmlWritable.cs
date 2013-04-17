using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore
{
    public static class XmlWritable
    {
        public static string ToXmlString(this IXmlWritable xmlWritable)
        {
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                xmlWritable.WriteTo(w);
                w.Flush();
                return sb.ToString();
            }
        }
    }
}
