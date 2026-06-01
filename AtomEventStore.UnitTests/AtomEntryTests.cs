using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Grean.AtomEventStore;
using AutoFixture.Xunit2;
using System.Xml;
using System.Xml.Linq;
using AutoFixture;
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

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(newTitle, actual.Title);
            Assert.Equal(sut.Published, actual.Published);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(sut.Content, actual.Content);
            Assert.Equal(sut.Links, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WithUpdatedReturnsCorrectResult(
            AtomEntry sut,
            DateTimeOffset newUpdated)
        {
            AtomEntry actual = sut.WithUpdated(newUpdated);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Published, actual.Published);
            Assert.Equal(newUpdated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(sut.Content, actual.Content);
            Assert.Equal(sut.Links, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WithAuthorReturnsCorrectResult(
            AtomEntry sut,
            AtomAuthor newAuthor)
        {
            AtomEntry actual = sut.WithAuthor(newAuthor);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Published, actual.Published);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(newAuthor, actual.Author);
            Assert.Equal(sut.Content, actual.Content);
            Assert.Equal(sut.Links, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WithContentReturnsCorrectResult(
            AtomEntry sut,
            XmlAtomContent newContent)
        {
            AtomEntry actual = sut.WithContent(newContent);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Published, actual.Published);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(newContent, actual.Content);
            Assert.Equal(sut.Links, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WithLinksReturnsCorrectResult(
            AtomEntry sut,
            IEnumerable<AtomLink> newLinks)
        {
            AtomEntry actual = sut.WithLinks(newLinks);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Published, actual.Published);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(sut.Content, actual.Content);
            Assert.Equal(newLinks, actual.Links);
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
            AtomEntry seed,
            XmlAttributedTestEventX tex,
            XmlContentSerializer serializer)
        {
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
            AtomEntry seed,
            XmlAttributedTestEventY tey,
            XmlContentSerializer serializer)
        {
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

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Published, actual.Published);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(sut.Content, actual.Content);
            Assert.Equal(sut.Links.Concat(new[] { newLink }), actual.Links);
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripToString(
            AtomEntryBuilder<XmlAttributedTestEventY> builder,
            XmlContentSerializer serializer)
        {
            var expected = builder.Build();
            var xml = expected.ToXmlString(serializer);

            AtomEntry actual = AtomEntry.Parse(xml, serializer);

            Assert.Equal(expected, actual, new AtomEntryComparer());
        }
    }
}
