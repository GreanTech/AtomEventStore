using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore
{
    public interface ITypeResolver
    {
        Type Resolve(string localName, string xmlNamespace);
    }
}
