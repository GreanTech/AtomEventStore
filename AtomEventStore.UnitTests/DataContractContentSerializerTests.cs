﻿using System;
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
            [Frozen]Mock<ITypeResolver> resolverStub,
            DataContractContentSerializer sut,
            DataContractTestEventX dctex)
        {
            resolverStub
                .Setup(r => r.Resolve("test-event-x", "http://grean.rocks/dc"))
                .Returns(dctex.GetType());

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
