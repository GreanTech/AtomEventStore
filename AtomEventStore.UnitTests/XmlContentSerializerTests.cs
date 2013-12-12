using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class XmlContentSerializerTests
    {
        [Theory, AutoAtomData]
        public void SutIsContentSerializer(XmlContentSerializer sut)
        {
            Assert.IsAssignableFrom<IContentSerializer>(sut);
        }
    }
}
