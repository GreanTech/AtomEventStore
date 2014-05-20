using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Ploeh.SemanticComparison.Fluent;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Ploeh.AutoFixture.Idioms;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomAuthorTests
    {
        [Theory, AutoAtomData]
        public void VerifyGuardClauses(GuardClauseAssertion assertion)
        {
            assertion.Verify(
                typeof(AtomAuthor).GetMembers().Where(m => m.Name != "WriteTo"));
        }

        [Theory, AutoAtomData]
        public void NameIsCorrectWhenModestConstructorIsUsed(
            [Frozen]string expected,
            [Modest]AtomAuthor sut)
        {
            string actual = sut.Name;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void WithNameReturnsCorrectResult(
            [Modest]AtomAuthor sut,
            string newName)
        {
            AtomAuthor actual = sut.WithName(newName);

            var expected = actual.AsSource().OfLikeness<AtomAuthor>()
                .With(x => x.Name).EqualsWhen(
                    (s, d) => object.Equals(newName, s.Name));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void SutEqualsIdenticalOther(AtomAuthor sut)
        {
            var other = sut.WithName(sut.Name);
            var actual = sut.Equals(other);
            Assert.True(actual);
        }

        [Theory, AutoAtomData]
        public void SutIsNotEqualToAnonymousObject(
            AtomAuthor sut,
            object anonymous)
        {
            var actual = sut.Equals(anonymous);
            Assert.False(actual);
        }

        [Theory, AutoAtomData]
        public void SutIsNotEqualToDifferentOther(
            AtomAuthor sut,
            AtomAuthor other)
        {
            var actual = sut.Equals(other);
            Assert.False(actual);
        }

        [Theory, AutoAtomData]
        public void GetHashCodeReturnsCorrectResult([Modest]AtomAuthor sut)
        {
            var actual = sut.GetHashCode();

            var expected = sut.Name.GetHashCode();
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void WriteToXmlWriterWritesCorrectXml(
            AtomAuthor sut)
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
                    "<author xmlns=\"http://www.w3.org/2005/Atom\">" +
                    "  <name>" + sut.Name + "</name>" +
                    "</author>");

                var actual = XDocument.Parse(sb.ToString());
                Assert.Equal(expected, actual, new XNodeEqualityComparer());
            }
            // Teardown
        }

        [Theory, AutoAtomData]
        public void SutIsXmlWritable(AtomAuthor sut)
        {
            Assert.IsAssignableFrom<IXmlWritable>(sut);
        }

        [Theory, AutoAtomData]
        public void ReadFromReturnsCorrectResult(
            AtomAuthor expected,
            IContentSerializer dummySerializer)
        {
            using (var sr = new StringReader(expected.ToXmlString(dummySerializer)))
            using (var r = XmlReader.Create(sr))
            {
                AtomAuthor actual = AtomAuthor.ReadFrom(r);
                Assert.Equal(expected, actual);
            }
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripToString(AtomAuthor expected)
        {
            var xml = expected.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());
            AtomAuthor actual = AtomAuthor.Parse(xml);
            Assert.Equal(expected, actual);
        }
    }
}
