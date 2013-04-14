using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;

namespace Grean.AtomEventStore.UnitTests
{
    public class InMemorySyndicationTests
    {
        [Theory, AutoAtomData]
        public void SutIsSyndicationFeedReader(InMemorySyndication sut)
        {
            Assert.IsAssignableFrom<ISyndicationFeedReader>(sut);
        }
    }
}
