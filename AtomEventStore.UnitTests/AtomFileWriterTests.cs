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
    public class AtomFileWriterTests
    {
        [Theory, AutoAtomData]
        public void SutIsSyndicationItemWriter(AtomFileWriter sut)
        {
            Assert.IsAssignableFrom<ISyndicationItemWriter>(sut);
        }

        [Theory, AutoAtomData]
        public void DirectoryIsCorrect(
            [Frozen]DirectoryInfo expected,
            AtomFileWriter sut)
        {
            DirectoryInfo actual = sut.Directory;
            Assert.Equal(expected, actual);
        }
    }
}
