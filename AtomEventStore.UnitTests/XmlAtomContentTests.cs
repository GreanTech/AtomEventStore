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
            XmlAtomContent content,
            AtomEntry entry,
            TestEventX tex)
        {
            // Fixture setup
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                var sut = content.WithItem(tex);

                // Exercise system
                sut.WriteTo(
                    w,
                    new ConventionBasedSerializerOfComplexImmutableClasses());

                // Verify outcome
                w.Flush();

                var expected = XDocument.Parse(
                    "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                    "  <test-event-x xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
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

        [Theory, AutoAtomData]
        public void SutCanRoundTripNestedEnumerable(
            XmlAtomContent seed,
            Guid id,
            Wrapper<TestEventX> w)
        {
            var expected = seed.WithItem(new Changeset<Wrapper<TestEventX>>(id, w));
            var xml = expected.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());

            var actual = XmlAtomContent.Parse(
                xml,
                new ConventionBasedSerializerOfComplexImmutableClasses());

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeEventWithDateTimeOffset(
            XmlAtomContent seed,
            TestEventD ted)
        {
            var sut = seed.WithItem(ted);

            var actual = sut.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());

            var expected = XDocument.Parse(
                "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "  <test-event-d xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
                "    <number>" + ted.Number + "</number>" +
                "    <date-time>" + ted.DateTime.ToString("o") + "</date-time>" +
                "  </test-event-d>" +
                "</content>");
            Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeEvenWithZeroedOutSubSeconds(
            DateTimeOffset dtSeed,
            XmlAtomContent seed,
            TestEventD ted)
        {
            /* The use of this particular constructor overload results in a
             * loss of precision, since ticks below the millisecond granularity
             * are lost. This causes ToString("o") to print trailing zeroes,
             * and reproduces a non-deterministically failing test, that
             * sometimes would fail because the actual implementation didn't 
             * print the trailing zeroes. */
            var dt = new DateTimeOffset(
                dtSeed.Year,
                dtSeed.Month,
                dtSeed.Day,
                dtSeed.Hour,
                dtSeed.Minute,
                dtSeed.Second,
                dtSeed.Millisecond,
                dtSeed.Offset);
            var sut = seed.WithItem(ted.WithDateTime(dt));

            var actual = sut.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());

            var expected = XDocument.Parse(
                "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "  <test-event-d xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
                "    <number>" + ted.Number + "</number>" +
                "    <date-time>" + dt.ToString("o") + "</date-time>" +
                "  </test-event-d>" +
                "</content>");
            Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripEventWithDateTimeOffset(
            XmlAtomContent seed,
            TestEventD ted)
        {
            var expected = seed.WithItem(ted);
            var xml = expected.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());

            var actual = XmlAtomContent.Parse(
                xml,
                new ConventionBasedSerializerOfComplexImmutableClasses());

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeSealedEvent(
            XmlAtomContent seed,
            TestEventSealed tes)
        {
            var sut = seed.WithItem(tes);

            var actual = sut.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());

            var expected = XDocument.Parse(
                "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "  <test-event-sealed xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
                "    <number>" + tes.Number + "</number>" +
                "    <text>" + tes.Text + "</text>" +
                "  </test-event-sealed>" +
                "</content>");
            Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripSealedEvent(
            XmlAtomContent seed,
            TestEventSealed tes)
        {
            var expected = seed.WithItem(tes);
            var xml = expected.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());

            var actual = XmlAtomContent.Parse(
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
