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
using System.Xml;
using System.Xml.Linq;
using System.IO;

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

        [Theory, AutoAtomData]
        public void SutEqualsIdenticalOther(XmlAtomContent sut)
        {
            var other = sut.WithItem(sut.Item);
            var actual = sut.Equals(other);
            Assert.True(actual);
        }

        [Theory, AutoAtomData]
        public void SutIsNotEqualToAnonymousObject(
            XmlAtomContent sut,
            object anonymous)
        {
            var actual = sut.Equals(anonymous);
            Assert.False(actual);
        }

        [Theory, AutoAtomData]
        public void SutDoesNotEqualDifferentOther(
            XmlAtomContent sut,
            XmlAtomContent other)
        {
            var actual = sut.Equals(other);
            Assert.False(actual);
        }

        [Theory, AutoAtomData]
        public void GetHashCodeReturnsCorrectResult(XmlAtomContent sut)
        {
            var actual = sut.GetHashCode();

            var expected = sut.Item.GetHashCode();
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void WriteToXmlWriterWritesCorrectXml(
            XmlContentSerializer serializer,
            XmlAtomContent content,
            AtomEntry entry,
            XmlAttributedTestEventX tex)
        {
            // Fixture setup
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                var sut = content.WithItem(tex);

                // Exercise system
                sut.WriteTo(w, serializer);

                // Verify outcome
                w.Flush();

                var expected = XDocument.Parse(
                    "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                    "  <test-event-x xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://grean:rocks\">" +
                    "    <number>" + tex.Number + "</number>" +
                    "    <text>" + tex.Text + "</text>" +
                    "  </test-event-x>" +
                    "</content>");

                var actual = XDocument.Parse(sb.ToString());
                Assert.Equal(expected, actual, new XNodeEqualityComparer());

                // Teardown
            }
        }

        [Theory, AutoAtomData]
        public void SutIsXmlWritable(XmlAtomContent sut)
        {
            Assert.IsAssignableFrom<IXmlWritable>(sut);
        }

        [Theory, AutoAtomData]
        public void ReadFromReturnsCorrectResult(
            XmlAtomContent seed,
            TestEventX tex)
        {
            var expected = seed.WithItem(tex);
            using (var sr = new StringReader(expected.ToXmlString(new ConventionBasedSerializerOfComplexImmutableClasses())))
            using (var r = XmlReader.Create(sr))
            {
                XmlAtomContent actual = XmlAtomContent.ReadFrom(r, new ConventionBasedSerializerOfComplexImmutableClasses());
                Assert.Equal(expected, actual);
            }
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripToString(
            XmlAtomContent seed,
            TestEventX tex)
        {
            var expected = seed.WithItem(tex);
            var xml = expected.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());

            XmlAtomContent actual = XmlAtomContent.Parse(
                xml,
                new ConventionBasedSerializerOfComplexImmutableClasses());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineAutoAtomData("<Content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">", "</Content>")]
        [InlineAutoAtomData("<foo type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">", "</foo>")]
        [InlineAutoAtomData("<content xmlns=\"http://www.w3.org/2005/Atom\">", "</content>")]
        [InlineAutoAtomData("<content type=\"Application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">", "</content>")]
        [InlineAutoAtomData("<content type=\"application/json\" xmlns=\"http://www.w3.org/2005/Atom\">", "</content>")]
        [InlineAutoAtomData("<content type=\"text/xml\" xmlns=\"http://www.w3.org/2005/Atom\">", "</content>")]
        [InlineAutoAtomData("<content type=\"bar\" xmlns=\"http://www.w3.org/2005/Atom\">", "</content>")]
        [InlineAutoAtomData("<content type=\"application/xml\" xmlns=\"baz\">", "</content>")]
        [InlineAutoAtomData("<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/ATOM\">", "</content>")]
        public void ParseThrowsOnWrongContainingElement(
            string startElement,
            string endElement,
            IContentSerializer dummySerializer)
        {
            var xml =
                startElement +
                "  <test-event-x xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
                "    <number>42</number>" +
                "    <text>Foo</text>" +
                "  </test-event-x>" +
                endElement;
            Assert.Throws<ArgumentException>(
                () => XmlAtomContent.Parse(
                    xml,
                    dummySerializer));
        }
    }
}
