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
using System.Xml;
using System.Xml.Linq;
using System.IO;

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

        [Theory, AutoAtomData]
        public void WithHrefReturnsCorrectResult(
            AtomLink sut,
            Uri newHref)
        {
            AtomLink actual = sut.WithHref(newHref);

            var expected = sut.AsSource().OfLikeness<AtomLink>()
                .With(x => x.Href).EqualsWhen(
                    (s, d) => object.Equals(newHref, d.Href));
            expected.ShouldEqual(actual);
        }

        [Theory]
        [InlineData(true, "ploeh", "ploeh", "fnaah", "fnaah")]
        [InlineData(false, "ndøh", "ploeh", "fnaah", "fnaah")]
        [InlineData(true, "ndøh", "ndøh", "fnaah", "fnaah")]
        [InlineData(true, "sgryt", "sgryt", "fnaah", "fnaah")]
        [InlineData(false, "sgryt", "sgryt", "pippo", "fnaah")]
        [InlineData(true, "sgryt", "sgryt", "pippo", "pippo")]
        public void EqualsReturnsCorrectResult(
            bool expected,
            string sutRel,
            string otherRel,
            string sutHref,
            string otherHref)
        {
            var sut = new AtomLink(sutRel, new Uri(sutHref, UriKind.Relative));
            var other = new AtomLink(otherRel, new Uri(otherHref, UriKind.Relative));

            var actual = sut.Equals(other);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutDoesNotEqualAnonymousOther(
            AtomLink sut,
            object anonymous)
        {
            var actual = sut.Equals(anonymous);
            Assert.False(actual);
        }

        [Theory, AutoAtomData]
        public void GetHashCodeReturnsCorrectResult(AtomLink sut)
        {
            var actual = sut.GetHashCode();

            var expected =
                sut.Rel.GetHashCode() ^
                sut.Href.GetHashCode();
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void WriteToXmlWriterWritesCorrectXml(
            AtomLink sut)
        {
            // Fixture setup
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                // Exercise system
                sut.WriteTo(w);

                // Verify outcome
                w.Flush();

                var expected = XDocument.Parse(
                    "<link" +
                    " href=\"" + sut.Href.ToString() + "\"" +
                    " rel=\"" + sut.Rel + "\"" +
                    " xmlns=\"http://www.w3.org/2005/Atom\" />");

                var actual = XDocument.Parse(sb.ToString());
                Assert.Equal(expected, actual, new XNodeEqualityComparer());
            }
            // Teardown
        }

        [Theory, AutoAtomData]
        public void SutIsXmlWritable(AtomLink sut)
        {
            Assert.IsAssignableFrom<IXmlWritable>(sut);
        }

        [Theory, AutoAtomData]
        public void ReadFromReturnsCorrectResult(
            AtomLink expected)
        {
            using (var sr = new StringReader(expected.ToXmlString()))
            using (var r = XmlReader.Create(sr))
            {
                AtomLink actual = AtomLink.ReadFrom(r);
                Assert.Equal(expected, actual);
            }
        }

        [Theory, AutoAtomData]
        public void ReadFromWhenHrefIsRelativeReturnsCorrectResult(
            AtomLink seed,
            string relativeUrl)
        {
            var expected = seed.WithHref(new Uri(relativeUrl, UriKind.Relative));
            using (var sr = new StringReader(expected.ToXmlString()))
            using (var r = XmlReader.Create(sr))
            {
                AtomLink actual = AtomLink.ReadFrom(r);
                Assert.Equal(expected, actual);
            }
        }

        [Theory, AutoAtomData]
        public void CreateSelfLinkReturnsCorrectResult(
            AtomLink link)
        {
            AtomLink actual = AtomLink.CreateSelfLink(link.Href);

            var expected = link.WithRel("self");
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void IsSelfLinkReturnsTrueForSelfLink(
            Uri href)
        {
            bool actual = AtomLink.CreateSelfLink(href).IsSelfLink;
            Assert.True(actual, "Should be self link.");
        }

        [Theory, AutoAtomData]
        public void IsSelfLinkReturnsFalsForUnSelfLink(
            AtomLink sut)
        {
            Assert.NotEqual("self", sut.Rel);
            var actual = sut.IsSelfLink;
            Assert.False(actual, "Should not be self link.");
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripToString(AtomLink expected)
        {
            var xml = expected.ToXmlString();
            AtomLink actual = AtomLink.Parse(xml);
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void CreateViaLinkReturnsCorrectResult(
            AtomLink link)
        {
            AtomLink actual = AtomLink.CreateViaLink(link.Href);

            var expected = link.WithRel("via");
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void IsViaLinkReturnsTrueForViaLink(
            Uri href)
        {
            bool actual = AtomLink.CreateViaLink(href).IsViaLink;
            Assert.True(actual, "Should be via link.");
        }

        [Theory, AutoAtomData]
        public void IsViaLinkReturnsFalsForNonViaLink(
            AtomLink sut)
        {
            Assert.NotEqual("via", sut.Rel);
            var actual = sut.IsViaLink;
            Assert.False(actual, "Should not be via link.");
        }

        [Theory, AutoAtomData]
        public void ToSelfLinkReturnsCorrectResult(
            AtomLink sut)
        {
            Assert.NotEqual("self", sut.Rel);

            AtomLink actual = sut.ToSelfLink();

            var expected = sut.WithRel("self");
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void ToViaLinkReturnsCorrectResult(
            AtomLink sut)
        {
            Assert.NotEqual("via", sut.Rel);

            AtomLink actual = sut.ToViaLink();

            var expected = sut.WithRel("via");
            Assert.Equal(expected, actual);
        }
    }
}
