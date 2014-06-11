using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Xunit;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Ploeh.AutoFixture.Xunit;
using Moq;

namespace Grean.AtomEventStore.UnitTests
{
    public class XmlContentSerializerTests
    {
        [Theory, AutoAtomData]
        public void SutIsContentSerializer(XmlContentSerializer sut)
        {
            Assert.IsAssignableFrom<IContentSerializer>(sut);
        }

        [Theory, AutoAtomData]
        public void SerializeCorrectlySerializesAttributedClassInstance(
            XmlContentSerializer sut,
            XmlAttributedTestEventX xate)
        {
            var sb = new StringBuilder();
            using(var w = XmlWriter.Create(sb))
            {
                sut.Serialize(w, xate);
                w.Flush();
                var actual = sb.ToString();

                var expected = XDocument.Parse(
                    "<test-event-x xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://grean:rocks\">" +
                    "  <number>" + xate.Number + "</number>" +
                    "  <text>" + xate.Text + "</text>" +
                    "</test-event-x>");
                Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
            }
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripAttributedClassInstance(
            XmlContentSerializer sut,
            XmlAttributedTestEventX xatex)
        {
            using (var ms = new MemoryStream())
            using (var w = XmlWriter.Create(ms))
            {
                sut.Serialize(w, xatex);
                w.Flush();
                ms.Position = 0;
                using(var r = XmlReader.Create(ms))
                {
                    var content = sut.Deserialize(r);

                    var actual = Assert.IsAssignableFrom<XmlAttributedTestEventX>(content.Item);
                    Assert.Equal(xatex.Number, actual.Number);
                    Assert.Equal(xatex.Text, actual.Text);
                }
            }
        }

        [Theory, AutoAtomData]
        public void NestedAttributedObjectIsCorrectlySerialized(
            XmlContentSerializer sut,
            XmlAttributedChangeset changeset,
            XmlAttributedTestEventX tex,
            XmlAttributedTestEventY tey)
        {
            changeset.Items = new object[] { tex, tey };

            var sb = new StringBuilder();
            using(var w = XmlWriter.Create(sb))
            {
                sut.Serialize(w, changeset);
                w.Flush();
                var actual = sb.ToString();

                var expected = XDocument.Parse(
                    "<changeset xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://grean:rocks\">" +
                    "  <id>" + changeset.Id + "</id>" +
                    "  <items>" +
                    "    <test-event-x>" +
                    "      <number>" + tex.Number + "</number>" +
                    "      <text>" + tex.Text + "</text>" +
                    "    </test-event-x>" +
                    "    <test-event-y>" +
                    "      <number>" + tey.Number + "</number>" +
                    "      <flag>" + tey.Flag.ToString().ToLowerInvariant() + "</flag>" +
                    "    </test-event-y>" +
                    "  </items>" +
                    "</changeset>");
                Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
            }
        }

        [Theory, AutoAtomData]
        public void NestedAttributedObjectCanRoundTrip(
            XmlContentSerializer sut,
            XmlAttributedChangeset changeset,
            XmlAttributedTestEventX tex,
            XmlAttributedTestEventY tey)
        {
            changeset.Items = new object[] { tex, tey };

            using (var ms = new MemoryStream())
            using (var w = XmlWriter.Create(ms))
            {
                sut.Serialize(w, changeset);
                w.Flush();
                ms.Position = 0;
                using (var r = XmlReader.Create(ms))
                {
                    var content = sut.Deserialize(r);

                    var actual = Assert.IsAssignableFrom<XmlAttributedChangeset>(content.Item);
                    Assert.Equal(changeset.Id, actual.Id);
                    var actualTex = Assert.IsAssignableFrom<XmlAttributedTestEventX>(actual.Items[0]);
                    Assert.Equal(tex.Number, actualTex.Number);
                    Assert.Equal(tex.Text, actualTex.Text);
                    var actualTey = Assert.IsAssignableFrom<XmlAttributedTestEventY>(actual.Items[1]);
                    Assert.Equal(tey.Number, actualTey.Number);
                    Assert.Equal(tey.Flag, actualTey.Flag);
                }
            }
        }
    }
}
