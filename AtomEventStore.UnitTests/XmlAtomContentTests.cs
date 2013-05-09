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
                sut.WriteTo(w);

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
            using (var sr = new StringReader(expected.ToXmlString()))
            using (var r = XmlReader.Create(sr))
            {
                XmlAtomContent actual = XmlAtomContent.ReadFrom(r);
                Assert.Equal(expected, actual);
            }
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripToString(
            XmlAtomContent seed,
            TestEventX tex)
        {
            var expected = seed.WithItem(tex);
            var xml = expected.ToXmlString();

            XmlAtomContent actual = XmlAtomContent.Parse(xml);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeNestedItem(
            XmlAtomContent seed,
            Envelope<TestEventX> env)
        {
            var sut = seed.WithItem(env);

            var actual = sut.ToXmlString();

            var expected = XDocument.Parse(
                "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "  <envelope xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
                "    <id>urn:uuid:" + env.Id + "</id>" +
                "    <item>" +
                "      <test-event-x>" +
                "        <number>" + env.Item.Number + "</number>" +
                "        <text>" + env.Item.Text + "</text>" +
                "      </test-event-x>" +
                "    </item>" +
                "  </envelope>" +
                "</content>");
            Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripNestedItem(
            XmlAtomContent seed,
            Envelope<TestEventY> env)
        {
            var expected = seed.WithItem(env);
            var xml = expected.ToXmlString();

            var actual = XmlAtomContent.Parse(xml);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeItemWithUri(
            XmlAtomContent seed,
            TestEventU teu)
        {
            var sut = seed.WithItem(teu);

            var actual = sut.ToXmlString();

            var expected = XDocument.Parse(
                "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "  <test-event-u xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
                "    <address>" + teu.Address.ToString() + "</address>" +
                "    <text>" + teu.Text + "</text>" +
                "  </test-event-u>" +
                "</content>");
            Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripItemWithUri(
            XmlAtomContent seed,
            TestEventU teu)
        {
            var expected = seed.WithItem(teu);
            var xml = expected.ToXmlString();

            var actual = XmlAtomContent.Parse(xml);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeDoublyNestedItem(
            XmlAtomContent seed,
            Envelope<Wrapper<TestEventX>> env)
        {
            var sut = seed.WithItem(env);

            var actual = sut.ToXmlString();

            var expected = XDocument.Parse(
                "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "  <envelope xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
                "    <id>urn:uuid:" + env.Id + "</id>" +
                "    <item>" +
                "      <wrapper>" +
                "        <item>" +
                "          <test-event-x>" +
                "            <number>" + env.Item.Item.Number + "</number>" +
                "            <text>" + env.Item.Item.Text + "</text>" +
                "          </test-event-x>" +
                "        </item>" +
                "      </wrapper>" +
                "    </item>" +
                "  </envelope>" +
                "</content>");
            Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripDoublyNestedItem(
            XmlAtomContent seed,
            Envelope<Wrapper<TestEventX>> env)
        {
            var expected = seed.WithItem(env);
            var xml = expected.ToXmlString();

            var actual = XmlAtomContent.Parse(xml);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeEventInSubNamespace(
            XmlAtomContent seed,
            SubNs.SubSubNs.TestEventS tes)
        {
            var sut = seed.WithItem(tes);

            var actual = sut.ToXmlString();

            var expected = XDocument.Parse(
                "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "  <test-event-s xmlns=\"urn:grean:atom-event-store:unit-tests:sub-ns:sub-sub-ns\">" +
                "    <number>" + tes.Number + "</number>" +
                "    <text>" + tes.Text + "</text>" +
                "  </test-event-s>" +
                "</content>");
            Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripEventInSubNamespace(
            XmlAtomContent seed,
            SubNs.SubSubNs.TestEventS tes)
        {
            var expected = seed.WithItem(tes);
            var xml = expected.ToXmlString();

            var actual = XmlAtomContent.Parse(xml);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeCompositeFromSeveralNamespaces(
            XmlAtomContent seed,
            Envelope<SubNs.SubSubNs.TestEventS> env)
        {
            var sut = seed.WithItem(env);

            var actual = sut.ToXmlString();

            var expected = XDocument.Parse(
                "<content type=\"application/xml\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "  <envelope xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
                "    <id>urn:uuid:" + env.Id + "</id>" +
                "    <item>" +
                "      <test-event-s xmlns=\"urn:grean:atom-event-store:unit-tests:sub-ns:sub-sub-ns\">" +
                "        <number>" + env.Item.Number + "</number>" +
                "        <text>" + env.Item.Text + "</text>" +
                "      </test-event-s>" +
                "    </item>" +
                "  </envelope>" +
                "</content>");
            Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripCompositeFromSeveralNamespaces(
            XmlAtomContent seed,
            Envelope<SubNs.SubSubNs.TestEventS> env)
        {
            var expected = seed.WithItem(env);
            var xml = expected.ToXmlString();

            var actual = XmlAtomContent.Parse(xml);

            Assert.Equal(expected, actual);
        }
    }
}
