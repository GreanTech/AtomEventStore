using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Xunit;
using Grean.AtomEventStore;
using AutoFixture;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomFeedTests
    {
        [Theory, AutoAtomData]
        public void IdIsCorrect([Frozen]UuidIri expected, AtomFeed sut)
        {
            UuidIri actual = sut.Id;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void TitleIsCorrect([Frozen]string expected, AtomFeed sut)
        {
            string actual = sut.Title;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void UpdatedIsCorrect(
            [Frozen]DateTimeOffset expected,
            AtomFeed sut)
        {
            DateTimeOffset actual = sut.Updated;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void AuthorIsCorrect(
            [Frozen]AtomAuthor expected,
            AtomFeed sut)
        {
            AtomAuthor actual = sut.Author;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void EntriesIsCorrect(
            [Frozen]IEnumerable<AtomEntry> expected,
            AtomFeed sut)
        {
            IEnumerable<AtomEntry> actual = sut.Entries;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void LinksIsCorrect(
            [Frozen]IEnumerable<AtomLink> expected,
            AtomFeed sut)
        {
            IEnumerable<AtomLink> actual = sut.Links;
            Assert.Equal(expected, actual);
        }

        [Theory, AutoAtomData]
        public void WithTitleReturnsCorrectResult(
            AtomFeed sut,
            string newTitle)
        {
            AtomFeed actual = sut.WithTitle(newTitle);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(newTitle, actual.Title);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(sut.Entries, actual.Entries);
            Assert.Equal(sut.Links, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WithUpdatedReturnsCorrectResult(
            AtomFeed sut,
            DateTimeOffset newUpdated)
        {
            AtomFeed actual = sut.WithUpdated(newUpdated);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(newUpdated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(sut.Entries, actual.Entries);
            Assert.Equal(sut.Links, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WithAuthorReturnsCorrectResult(
            AtomFeed sut,
            AtomAuthor newAuthor)
        {
            AtomFeed actual = sut.WithAuthor(newAuthor);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(newAuthor, actual.Author);
            Assert.Equal(sut.Entries, actual.Entries);
            Assert.Equal(sut.Links, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WithEntriesReturnsCorrectResult(
            AtomFeed sut,
            IEnumerable<AtomEntry> newEntries)
        {
            AtomFeed actual = sut.WithEntries(newEntries);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(newEntries, actual.Entries);
            Assert.Equal(sut.Links, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WithLinksReturnsCorrectResult(
            AtomFeed sut,
            IEnumerable<AtomLink> newLinks)
        {
            AtomFeed actual = sut.WithLinks(newLinks);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(sut.Entries, actual.Entries);
            Assert.Equal(newLinks, actual.Links);
        }

        [Theory, AutoAtomData]
        public void WriteToXmlWriterWritesCorrectXml(
            XmlContentSerializer serializer,
            AtomXmlWriter<XmlContentSerializer> writer,
            AtomFeed feed,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            // Fixture setup
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                var entries = feed.Entries.Zip(
                    eventGenerator,
                    (entry, @event) => entry.WithContent(
                        entry.Content.WithItem(@event))).ToList();
                var sut = feed.WithEntries(entries);

                // Exercise system
                sut.WriteTo(w, serializer);

                // Verify outcome
                w.Flush();

                var expectedLinks = string.Concat(sut.Links.Select(writer.ToXml));
                var expectedEntries = string.Concat(entries.Select(writer.ToXml));

                var expected = XDocument.Parse(
                    "<feed xmlns=\"http://www.w3.org/2005/Atom\">" +
                    "  <id>" + sut.Id.ToString() + "</id>" +
                    "  <title type=\"text\">" + sut.Title + "</title>" +
                    "  <updated>" + sut.Updated.ToString("o") + "</updated>" +
                    "  <author>" +
                    "    <name>" + sut.Author.Name + "</name>" +
                    "  </author>" +
                    expectedLinks +
                    expectedEntries +
                    "</feed>");

                var actual = XDocument.Parse(sb.ToString());
                Assert.Equal(expected, actual, new XNodeEqualityComparer());
            }
            // Teardown
        }

        [Theory, AutoAtomData]
        public void SutIsXmlWritable(AtomFeed sut)
        {
            Assert.IsAssignableFrom<IXmlWritable>(sut);
        }

        [Theory, AutoAtomData]
        public void ReadFromReturnsCorrectResult(
            XmlContentSerializer serializer,
            AtomFeed feed,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var entries = feed.Entries.Zip(
                eventGenerator,
                (entry, @event) => entry.WithContent(
                    entry.Content.WithItem(@event))).ToList();
            var expected = feed.WithEntries(entries);

            using (var sr = new StringReader(expected.ToXmlString(serializer)))
            using (var r = XmlReader.Create(sr))
            {
                AtomFeed actual = AtomFeed.ReadFrom(r, serializer);
                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }

        [Theory, AutoAtomData]
        public void AddLinkReturnsCorrectResult(
            AtomFeed sut,
            AtomLink newLink)
        {
            AtomFeed actual = sut.AddLink(newLink);

            Assert.Equal(sut.Id, actual.Id);
            Assert.Equal(sut.Title, actual.Title);
            Assert.Equal(sut.Updated, actual.Updated);
            Assert.Equal(sut.Author, actual.Author);
            Assert.Equal(sut.Entries, actual.Entries);
            Assert.Equal(sut.Links.Concat(new[] { newLink }), actual.Links);
        }

        [Theory, AutoAtomData]
        public void SutCanRoundTripToString(
            XmlContentSerializer serializer,
            AtomFeedBuilder<XmlAttributedTestEventY> builder)
        {
            var expected = builder.Build();
            var xml = expected.ToXmlString(serializer);

            AtomFeed actual = AtomFeed.Parse(xml, serializer);

            Assert.Equal(expected, actual, new AtomFeedComparer());
        }
    }
}
