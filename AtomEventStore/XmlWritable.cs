﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore
{
    public static class XmlWritable
    {
        public static string ToXmlString(
            this IXmlWritable xmlWritable,
            IContentSerializer serializer)
        {
            return xmlWritable.ToXmlString(serializer, new XmlWriterSettings());
        }

        public static string ToXmlString(
            this IXmlWritable xmlWritable,
            IContentSerializer serializer,
            XmlWriterSettings settings)
        {
            if (xmlWritable == null)
                throw new ArgumentNullException("xmlWritable");

            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb, settings))
            {
                xmlWritable.WriteTo(w, serializer);
                w.Flush();
                return sb.ToString();
            }
        }
    }
}
