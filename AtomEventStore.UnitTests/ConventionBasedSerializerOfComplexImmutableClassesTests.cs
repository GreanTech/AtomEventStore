﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using Xunit.Extensions;

namespace Grean.AtomEventStore.UnitTests
{
    public class ConventionBasedSerializerOfComplexImmutableClassesTests
    {
        [Theory, AutoAtomData]
        public void SutCanSerializeNestedItem(
            ConventionBasedSerializerOfComplexImmutableClasses sut,
            XmlAtomContent seed,
            Envelope<TestEventX> env)
        {
            var content = seed.WithItem(env);

            var actual = content.ToXmlString(sut);

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
            ConventionBasedSerializerOfComplexImmutableClasses sut,
            XmlAtomContent seed,
            Envelope<TestEventY> env)
        {
            var expected = seed.WithItem(env);
            var xml = expected.ToXmlString(sut);

            var actual = XmlAtomContent.Parse(xml, sut);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeItemWithUri(
            ConventionBasedSerializerOfComplexImmutableClasses sut,
            XmlAtomContent seed,
            TestEventU teu)
        {
            var content = seed.WithItem(teu);

            var actual = content.ToXmlString(sut);

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
            ConventionBasedSerializerOfComplexImmutableClasses sut,
            XmlAtomContent seed,
            TestEventU teu)
        {
            var expected = seed.WithItem(teu);
            var xml = expected.ToXmlString(sut);

            var actual = XmlAtomContent.Parse(xml, sut);

            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void SutCanSerializeDoublyNestedItem(
            ConventionBasedSerializerOfComplexImmutableClasses sut,
            XmlAtomContent seed,
            Envelope<Wrapper<TestEventX>> env)
        {
            var content = seed.WithItem(env);

            var actual = content.ToXmlString(sut);

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
    }
}
