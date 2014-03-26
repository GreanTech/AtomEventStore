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

namespace Grean.AtomEventStore.UnitTests
{
    public class DataContractContentSerializerTests
    {
        [Theory, AutoAtomData]
        public void SutIsContentSerializer(DataContractContentSerializer sut)
        {
            Assert.IsAssignableFrom<IContentSerializer>(sut);
        }

        [Theory, AutoAtomData]
        public void SerializeCorrectlySerializesAttributedClassInstance(
            DataContractContentSerializer sut,
            DataContractTestEventX xate)
        {
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                sut.Serialize(w, xate);
                w.Flush();
                var actual = sb.ToString();

                var expected = XDocument.Parse(
                    "<test-event-x xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://grean.rocks/dc\">" +
                    "  <number>" + xate.Number + "</number>" +
                    "  <text>" + xate.Text + "</text>" +
                    "</test-event-x>");
                Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
            }
        }
    }
}
