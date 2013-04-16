using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Xunit;
using Ploeh.SemanticComparison.Fluent;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomLinkTests
    {
        [Theory, AutoAtomData]
        public void RelIsCorrect([Frozen]string expected, AtomLink sut)
        {
            string actual = sut.Rel;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void HrefIsCorrect([Frozen]Uri expected, AtomLink sut)
        {
            Uri actual = sut.Href;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void WithRelReturnsCorrectResult(
            AtomLink sut,
            string newRel)
        {
            AtomLink actual = sut.WithRel(newRel);

            var expected = sut.AsSource().OfLikeness<AtomLink>()
                .With(x => x.Rel).EqualsWhen(
                    (s, d) => object.Equals(newRel, d.Rel));
            expected.ShouldEqual(actual);
        }
    }
}
