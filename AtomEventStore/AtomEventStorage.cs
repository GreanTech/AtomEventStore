using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore
{
    /// <summary>
    /// Contains helper methods for Atom event storage.
    /// </summary>
    public static class AtomEventStorage
    {

        /// <summary>
        /// Creates a new, empty feed.
        /// </summary>
        /// <param name="href">The address of the feed.</param>
        /// <returns>A new, empty Atom Feed.</returns>
        /// <remarks>
        /// <para>
        /// <paramref name="href" /> is expected to contain an
        /// <see cref="Uri" /> in the last segment. This Uri is used as the ID
        /// for the new Atom Feed.
        /// </para>
        /// </remarks>
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

        internal static Guid GetIdFromHref(Uri href)
        {
            var segments = GetSegmentsFrom(href);
            // The ID is assumed to be contained in the last segment of the URL
            var lastSegment = segments.Last();
            return new Guid(lastSegment);
        }

        internal static string[] GetSegmentsFrom(Uri href)
        {
            /* The assumption here is that the href argument is always going to
             * be a relative URL. So far at least, that's consistent with how
             * AtomEventStore works.
             * However, the Segments property only works for absolute URLs. */
            var fakeBase = new Uri("http://grean.com");
            var absoluteHref = new Uri(fakeBase, href);
            return absoluteHref.Segments;
        }
    }
}
