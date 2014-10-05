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
using Ploeh.AutoFixture.Xunit;
using Moq;
using System.IO;
using System.Runtime.Serialization;

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
            DataContractTestEventX dctex)
        {
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                sut.Serialize(w, dctex);
                w.Flush();
                var actual = sb.ToString();

                var expected = XDocument.Parse(
                    "<test-event-x xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://grean.rocks/dc\">" +
                    "  <number>" + dctex.Number + "</number>" +
                    "  <text>" + dctex.Text + "</text>" +
                    "</test-event-x>");
                Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
            }
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripAttributedClassInstance(
            DataContractContentSerializer sut,
            DataContractTestEventX dctex)
        {
            using (var ms = new MemoryStream())
            using (var w = XmlWriter.Create(ms))
            {
                sut.Serialize(w, dctex);
                w.Flush();
                ms.Position = 0;
                using (var r = XmlReader.Create(ms))
                {
                    var content = sut.Deserialize(r);

                    var actual = Assert.IsAssignableFrom<DataContractTestEventX>(content.Item);
                    Assert.Equal(dctex.Number, actual.Number);
                    Assert.Equal(dctex.Text, actual.Text);
                }
            }
        }

        [Fact]
        public void CreateWithNullAssemblyThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DataContractContentSerializer.Create(null));
        }

        [Fact]
        public void CreateWithAssemblyWithoutAnnotatedTypesThrows()
        {
            var assembly = typeof(Version).Assembly;
            Assert.Empty(
                from t in assembly.GetTypes()
                from a in t.GetCustomAttributes(
                              typeof(DataContractAttribute), inherit: false)
                           .Cast<DataContractAttribute>()
                where t.IsDefined(a.GetType(), inherit: false)
                select t);

            Assert.Throws<InvalidOperationException>(() =>
                DataContractContentSerializer.Create(assembly));
        }

        [Theory, AutoData]
        public void CreateWithAssemblyCorrectlySerializesAttributedClassInstance(
            DataContractTestEventX dctex)
        {
            var assembly = typeof(DataContractTestEventX).Assembly;
            Assert.NotEmpty(
                from t in assembly.GetTypes()
                from a in t.GetCustomAttributes(
                              typeof(DataContractAttribute), inherit: false)
                           .Cast<DataContractAttribute>()
                where t.IsDefined(a.GetType(), inherit: false)
                select t);
            var sut = DataContractContentSerializer.Create(assembly);

            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                sut.Serialize(w, dctex);
                w.Flush();
                var actual = sb.ToString();

                var expected = XDocument.Parse(
                    "<test-event-x xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://grean.rocks/dc\">" +
                    "  <number>" + dctex.Number + "</number>" +
                    "  <text>" + dctex.Text + "</text>" +
                    "</test-event-x>");
                Assert.Equal(expected, XDocument.Parse(actual), new XNodeEqualityComparer());
            }
        }

        [Theory, AutoData]
        public void CreateWithAssemblyCanRoundTripAttributedClassInstance(
            DataContractTestEventX dctex)
        {
            var assembly = typeof(DataContractTestEventX).Assembly;
            Assert.NotEmpty(
                from t in assembly.GetTypes()
                from a in t.GetCustomAttributes(
                              typeof(DataContractAttribute), inherit: false)
                           .Cast<DataContractAttribute>()
                where t.IsDefined(a.GetType(), inherit: false)
                select t);
            var sut = DataContractContentSerializer.Create(assembly);

            using (var ms = new MemoryStream())
            using (var w = XmlWriter.Create(ms))
            {
                sut.Serialize(w, dctex);
                w.Flush();
                ms.Position = 0;
                using (var r = XmlReader.Create(ms))
                {
                    var content = sut.Deserialize(r);

                    var actual = Assert.IsAssignableFrom<DataContractTestEventX>(content.Item);
                    Assert.Equal(dctex.Number, actual.Number);
                    Assert.Equal(dctex.Text, actual.Text);
                }
            }
        }
    }
}
