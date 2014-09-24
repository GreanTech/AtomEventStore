using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore
{
    public class TypeResolutionTable : ITypeResolver
    {
        public Type Resolve(string localName, string xmlNamespace)
        {
            if (localName == null)
                throw new ArgumentNullException("localName");

            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");

            throw new NotImplementedException();
        }
    }
}
