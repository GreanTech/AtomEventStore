using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace Grean.AtomEventStore.UnitTests
{
    public class LifoEventsTests
    {
        [Theory, AutoAtomData]
        public void SutIsEnumerable(LifoEvents<XmlAttributedTestEventX> sut)
        {
            Assert.IsAssignableFrom<IEnumerable<XmlAttributedTestEventX>>(sut);
        }
    }
}
