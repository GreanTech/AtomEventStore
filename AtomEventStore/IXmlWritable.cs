using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public interface IXmlWritable
    {
        void WriteTo(XmlWriter xmlWriter);
    }
}
