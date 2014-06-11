using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Xunit;
using Ploeh.SemanticComparison.Fluent;
using System.Xml;
using System.Xml.Linq;
using Ploeh.AutoFixture;
using System.IO;
using Moq;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEntryTests
    {
        [Theory, AutoAtomData]
        public void IdIsCorrect([Frozen]UuidIri expected, AtomEntry sut)
        {
            UuidIri actual = sut.Id;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void TitleIsCorrect([Frozen]string expected, AtomEntry sut)
        {
            string actual = sut.Title;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void PublishedReturnsCorrectResult(
            [Frozen]DateTimeOffset expected,
            AtomEntry sut)
        {
            DateTimeOffset actual = sut.Published;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void UpdatedReturnsCorrectResult(
            [Frozen]DateTimeOffset expected,
            AtomEntry sut)
        {
            DateTimeOffset actual = sut.Updated;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void PublishedAndUpdatedCanBeDifferent(AtomEntry sut)
        {
            Assert.NotEqual(sut.Published, sut.Updated);
        }

        [Theory, AutoAtomData]
        public void AuthorIsCorrect([Frozen]AtomAuthor expected, AtomEntry sut)
        {
            AtomAuthor actual = sut.Author;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void ContentIsCorrect(
            [Frozen]XmlAtomContent expected,
            AtomEntry sut)
        {
            XmlAtomContent actual = sut.Content;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void LinksIsCorrect(
            [Frozen]IEnumerable<AtomLink> expected,
            AtomEntry sut)
        {
            IEnumerable<AtomLink> actual = sut.Links;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void WithTitleReturnsCorrectResult(
            AtomEntry sut,
            string newTitle)
        {
            AtomEntry actual = sut.WithTitle(newTitle);

            var expected = sut.AsSource().OfLikeness<AtomEntry>()
                .With(x => x.Title).EqualsWhen(
                    (s, d) => object.Equals(newTitle, d.Title));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WithUpdatedReturnsCorrectResult(
            AtomEntry sut,
            DateTimeOffset newUpdated)
        {
            AtomEntry actual = sut.WithUpdated(newUpdated);

            var expected = sut.AsSource().OfLikeness<AtomEntry>()
                .With(x => x.Updated).EqualsWhen(
                    (s, d) => object.Equals(newUpdated, d.Updated));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WithAuthorReturnsCorrectResult(
            AtomEntry sut,
            AtomAuthor newAuthor)
        {
            AtomEntry actual = sut.WithAuthor(newAuthor);

            var expected = sut.AsSource().OfLikeness<AtomEntry>()
                .With(x => x.Author).EqualsWhen(
                    (s, d) => object.Equals(newAuthor, d.Author));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WithContentReturnsCorrectResult(
            AtomEntry sut,
            XmlAtomContent newContent)
        {
            AtomEntry actual = sut.WithContent(newContent);

            var expected = sut.AsSource().OfLikeness<AtomEntry>()
                .With(x => x.Content).EqualsWhen(
                    (s, d) => object.Equals(newContent, d.Content));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WithLinksReturnsCorrectResult(
            AtomEntry sut,
            IEnumerable<AtomLink> newLinks)
        {
            AtomEntry actual = sut.WithLinks(newLinks);

            var expected = sut.AsSource().OfLikeness<AtomEntry>()
                .With(x => x.Links).EqualsWhen(
                    (s, d) => newLinks.SequenceEqual(d.Links));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WriteToXmlWriterWritesCorrectXml(
            AtomEntry entry,
            Generator<AtomLink> linkGenerator,
            DataContractTestEventX tex,
            DataContractContentSerializer serializer)
        {
            // Fixture setup
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                var links = linkGenerator.Take(2).ToList();
                var sut = entry
                    .WithContent(new XmlAtomContent(tex))
                    .WithLinks(links);

                // Exercise system
                sut.WriteTo(w, serializer);

                // Verify outcome
                w.Flush();

                var expected = XDocument.Parse(
                    "<entry xmlns=\"http://www.w3.org/2005/Atom\">" +
                    "  <id>" + sut.Id.ToString() + "</id>" +
                    "  <title type=\"text\">" + sut.Title + "</title>" +
                    "  <published>" + sut.Published.ToString("o") + "</published>" +
                    "  <updated>" + sut.Updated.ToString("o") + "</updated>" +
                    "  <author>" +
                    "    <name>" + sut.Author.Name + "</name>" +
                    "  </author>" +
                    "  <link href=\"" + links[0].Href.ToString() + "\" rel=\"" + links[0].Rel + "\" />" +
                    "  <link href=\"" + links[1].Href.ToString() + "\" rel=\"" + links[1].Rel + "\" />" +
                    "  <content type=\"application/xml\">" +
                    "    <test-event-x xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://grean.rocks/dc\">" +
                    "      <number>" + tex.Number + "</number>" +
                    "      <text>" + tex.Text + "</text>" +
                    "    </test-event-x>" +
                    "  </content>" +
                    "</entry>");

                var actual = XDocument.Parse(sb.ToString());
                Assert.Equal(expected, actual, new XNodeEqualityComparer());

                // Teardown
            }
        }

        [Theory, AutoAtomData]
        public void SutIsXmlWritable(AtomEntry sut)
        {
            Assert.IsAssignableFrom<IXmlWritable>(sut);
        }

        [Theory, AutoAtomData]
        public void ReadFromReturnsCorrectResult(
            [Frozen]Mock<ITypeResolver> resolverStub,
            AtomEntry seed,
            XmlAttributedTestEventX tex,
            XmlContentSerializer serializer)
        {
            resolverStub
                .Setup(r => r.Resolve("test-event-x", "http://grean:rocks"))
                .Returns(tex.GetType());
            var expected = seed.WithContent(seed.Content.WithItem(tex));

            using (var sr = new StringReader(expected.ToXmlString(serializer)))
            using (var r = XmlReader.Create(sr))
            {
                AtomEntry actual = AtomEntry.ReadFrom(r, serializer);
                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }

        [Theory, AutoAtomData]
        public void ReadFromXmlWithTestEventYReturnsCorrectResult(
            [Frozen]Mock<ITypeResolver> resolverStub,
            AtomEntry seed,
            XmlAttributedTestEventY tey,
            XmlContentSerializer serializer)
        {
            resolverStub
                .Setup(r => r.Resolve("test-event-y", "http://grean:rocks"))
                .Returns(tey.GetType());
            var expected = seed.WithContent(seed.Content.WithItem(tey));

            using (var sr = new StringReader(expected.ToXmlString(serializer)))
            using (var r = XmlReader.Create(sr))
            {
                AtomEntry actual = AtomEntry.ReadFrom(r, serializer);
                Assert.Equal(expected, actual, new AtomEntryComparer());
            }
        }

        [Theory, AutoAtomData]
        public void AddLinkReturnsCorrectResult(
            AtomEntry sut,
            AtomLink newLink)
        {
            AtomEntry actual = sut.AddLink(newLink);

            var expected = sut.AsSource().OfLikeness<AtomEntry>()
                .With(x => x.Links).EqualsWhen(
                    (s, d) => sut.Links.Concat(new[] { newLink }).SequenceEqual(d.Links));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripToString(AtomEntryBuilder<TestEventY> builder)
        {
            var expected = builder.Build();
            var xml = expected.ToXmlString(
                new ConventionBasedSerializerOfComplexImmutableClasses());

            AtomEntry actual = AtomEntry.Parse(
                xml,
                new ConventionBasedSerializerOfComplexImmutableClasses());

            Assert.Equal(expected, actual, new AtomEntryComparer());
        }
    }
}
