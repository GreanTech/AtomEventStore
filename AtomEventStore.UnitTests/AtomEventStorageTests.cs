using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit;
using Xunit.Extensions;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEventStorageTests
    {
        [Theory, AutoAtomData]
        public void CreateNewFeedReturnsCorrectResult(
            Guid id,
            IContentSerializer dummySerializer)
        {
            var href = new Uri(id.ToString(), UriKind.Relative);

            XmlReader actual = AtomEventStorage.CreateNewFeed(href);

            var expected = XDocument.Parse(
                new AtomFeed(
                    id,
                    "Index of event stream " + id,
                    DateTimeOffset.Now,
                    new AtomAuthor("Grean"),
                    Enumerable.Empty<AtomEntry>(),
                    new[]
                    {
                        AtomLink.CreateSelfLink(href)
                    })
                .ToXmlString(dummySerializer));
            Assert.Equal(expected, XDocument.Load(actual), new XNodeEqualityComparer());
        }
    }
}
