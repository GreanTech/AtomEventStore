using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;
using Ploeh.SemanticComparison.Fluent;

namespace Grean.AtomEventStore.UnitTests
{
    public class XmlAtomContentTests
    {
        [Theory, AutoAtomData]
        public void ItemIsCorrect(
            [Frozen]object expected,
            XmlAtomContent sut)
        {
            var actual = sut.Item;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void WithItemReturnsCorrectResult(
            XmlAtomContent sut,
            object newItem)
        {
            XmlAtomContent actual = sut.WithItem(newItem);

            var expected = actual.AsSource().OfLikeness<XmlAtomContent>()
                .With(x => x.Item).EqualsWhen(
                    (s, d) => object.Equals(newItem, s.Item));
            expected.ShouldEqual(actual);
        }
    }
}
