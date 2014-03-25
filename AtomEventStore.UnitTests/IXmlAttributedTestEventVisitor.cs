using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.UnitTests
{
    public interface IXmlAttributedTestEventVisitor
    {
        IXmlAttributedTestEventVisitor Visit(XmlAttributedTestEventX tex);

        IXmlAttributedTestEventVisitor Visit(XmlAttributedTestEventY tey);
    }
}
