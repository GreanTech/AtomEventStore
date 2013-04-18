using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.SemanticComparison.Fluent;
using Ploeh.AutoFixture;
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

            var expected = sut.AsSource().OfLikeness<AtomFeed>()
                .With(x => x.Title).EqualsWhen(
                    (s, d) => object.Equals(newTitle, d.Title));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WithUpdatedReturnsCorrectResult(
            AtomFeed sut,
            DateTimeOffset newUpdated)
        {
            AtomFeed actual = sut.WithUpdated(newUpdated);

            var expected = sut.AsSource().OfLikeness<AtomFeed>()
                .With(x => x.Updated).EqualsWhen(
                    (s, d) => object.Equals(newUpdated, d.Updated));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WithAuthorReturnsCorrectResult(
            AtomFeed sut,
            AtomAuthor newAuthor)
        {
            AtomFeed actual = sut.WithAuthor(newAuthor);

            var expected = sut.AsSource().OfLikeness<AtomFeed>()
                .With(x => x.Author).EqualsWhen(
                    (s, d) => object.Equals(newAuthor, d.Author));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WithEntriesReturnsCorrectResult(
            AtomFeed sut,
            IEnumerable<AtomEntry> newEntries)
        {
            AtomFeed actual = sut.WithEntries(newEntries);

            var expected = sut.AsSource().OfLikeness<AtomFeed>()
                .With(x => x.Entries).EqualsWhen(
                    (s, d) => newEntries.SequenceEqual(d.Entries));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WithLinksReturnsCorrectResult(
            AtomFeed sut,
            IEnumerable<AtomLink> newLinks)
        {
            AtomFeed actual = sut.WithLinks(newLinks);

            var expected = sut.AsSource().OfLikeness<AtomFeed>()
                .With(x => x.Links).EqualsWhen(
                    (s, d) => newLinks.SequenceEqual(d.Links));
            expected.ShouldEqual(actual);
        }

        [Theory, AutoAtomData]
        public void WriteToXmlWriterWritesCorrectXml(
            AtomFeed feed,
            Generator<TestEventX> eventGenerator)
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
                sut.WriteTo(w);

                // Verify outcome
                w.Flush();

                var expectedLinks = string.Concat(sut.Links.Select(ToXml));
                var expectedEntries = string.Concat(entries.Select(ToXml));

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

        private static string ToXml(AtomLink link)
        {
            return link
                .ToXmlString(new XmlWriterSettings { OmitXmlDeclaration = true })
                .Replace("xmlns=\"http://www.w3.org/2005/Atom\"", "");
        }

        private static string ToXml(AtomEntry entry)
        {
            return entry
                .ToXmlString(new XmlWriterSettings { OmitXmlDeclaration = true })
                .Replace("xmlns=\"http://www.w3.org/2005/Atom\"", "");
        }

        [Theory, AutoAtomData]
        public void SutIsXmlWritable(AtomFeed sut)
        {
            Assert.IsAssignableFrom<IXmlWritable>(sut);
        }

        [Theory, AutoAtomData]
        public void ReadFromReturnsCorrectResult(
            AtomFeed feed,
            Generator<TestEventX> eventGenerator)
        {
            var entries = feed.Entries.Zip(
                eventGenerator,
                (entry, @event) => entry.WithContent(
                    entry.Content.WithItem(@event))).ToList();
            var expected = feed.WithEntries(entries);

            using (var sr = new StringReader(expected.ToXmlString()))
            using (var r = XmlReader.Create(sr))
            {
                AtomFeed actual = AtomFeed.ReadFrom(r);
                Assert.Equal(expected, actual, new AtomFeedComparer());
            }
        }

        [Theory, AutoAtomData]
        public void AddLinkReturnsCorrectResult(
            AtomFeed sut,
            AtomLink newLink)
        {
            AtomFeed actual = sut.AddLink(newLink);

            var expected = sut.AsSource().OfLikeness<AtomFeed>()
                .With(x => x.Links).EqualsWhen(
                    (s, d) => sut.Links.Concat(new[] { newLink }).SequenceEqual(d.Links));
            expected.ShouldEqual(actual);
        }
    }
}
