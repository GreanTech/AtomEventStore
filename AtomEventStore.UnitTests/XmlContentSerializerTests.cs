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
                    "<test-event xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://grean:rocks\">" +
                    "  <number>" + xate.Number + "</number>" +
                    "  <text>" + xate.Text + "</text>" +
                    "</test-event>");
                Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
            }
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripAttributedClassInstance(
            [Frozen]Mock<ITypeResolver> resolverStub,
            XmlContentSerializer sut,
            XmlAttributedTestEventX xatex)
        {
            resolverStub
                .Setup(r => r.Resolve("test-event", "http://grean:rocks"))
                .Returns(xatex.GetType());

            using(var ms = new MemoryStream())
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
    }
}
