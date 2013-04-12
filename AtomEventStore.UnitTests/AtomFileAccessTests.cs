using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Xunit;
using Ploeh.AutoFixture.Xunit;
using System.IO;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomFileAccessTests
    {
        [Theory, AutoAtomData]
        public void SutIsSyndicationItemWriter(AtomFileAccess sut)
        {
            Assert.IsAssignableFrom<ISyndicationItemWriter>(sut);
        }

        [Theory, AutoAtomData]
        public void SutIsSyndicationFeedWriter(AtomFileAccess sut)
        {
            Assert.IsAssignableFrom<ISyndicationFeedWriter>(sut);
        }

        [Theory, AutoAtomData]
        public void DirectoryIsCorrect(
            [Frozen]DirectoryInfo expected,
            AtomFileAccess sut)
        {
            DirectoryInfo actual = sut.Directory;
            Assert.Equal(expected, actual);
        }
    }
}
