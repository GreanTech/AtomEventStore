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
            return new Guid(href.ToString());
        }
    }
}
