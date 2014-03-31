using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore
{
    public static class AtomEventStorage
    {

        public static XmlReader CreateNewFeed(Uri href)
        {
            var id = GetIdFromHref(href);
            var xml = new AtomFeed(
                id,
                "Index of event stream " + id,
                DateTimeOffset.Now,
                new AtomAuthor("Grean"),
                Enumerable.Empty<AtomEntry>(),
                new[]
                {
                    AtomLink.CreateSelfLink(href)
                })
                .ToXmlString((IContentSerializer)null);

            var sr = new StringReader(xml);
            try
            {
                return XmlReader.Create(
                    sr,
                    new XmlReaderSettings { CloseInput = true });
            }
            catch
            {
                sr.Dispose();
                throw;
            }
        }

        private static Guid GetIdFromHref(Uri href)
        {
            /* The assumption here is that the href argument is always going to
             * be a relative URL. So far at least, that's consistent with how
             * AtomEventStore works.
             * However, the Segments property only works for absolute URLs. */
            var fakeBase = new Uri("http://grean.com");
            var absoluteHref = new Uri(fakeBase, href);
            // The ID is assumed to be contained in the last segment of the URL
            var lastSegment = absoluteHref.Segments.Last();
            return new Guid(lastSegment);
        }
    }
}
