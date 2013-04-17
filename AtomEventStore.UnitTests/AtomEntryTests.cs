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
            TestEventX tex)
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
                sut.WriteTo(w);

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
                    "    <test-event-x xmlns=\"urn:grean:atom-event-store:unit-tests\">" +
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
    }
}
