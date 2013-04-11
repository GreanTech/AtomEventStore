using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Grean.AtomEventStore.UnitTests
{
    public class SyndicationItemResemblance : SyndicationItem
    {
        private readonly SyndicationContentComparer contentComparer;

        public SyndicationItemResemblance(SyndicationItem item)
            : base(item)
        {
            this.contentComparer = new SyndicationContentComparer();
        }

        public override bool Equals(object obj)
        {
            var other = obj as SyndicationItem;
            if (other != null)
                return IsCorrectId(other.Id)
                    && HasCorrectTitle(other)
                    && this.HasCorrectLinks(other)
                    && this.HasCorrectDates(other)
                    && HasCorrectAuthors(other.Authors)
                    && this.contentComparer.Equals(
                        this.Content, other.Content);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override SyndicationItem Clone()
        {
            return new SyndicationItemResemblance(this);
        }

        private static bool IsCorrectId(string candidate)
        {
            UuidIri dummy;
            return UuidIri.TryParse(candidate, out dummy);
        }

        private static bool HasCorrectAuthors(
            IEnumerable<SyndicationPerson> candidate)
        {
            return candidate.Any(p => !string.IsNullOrWhiteSpace(p.Name));
        }

        private static bool HasCorrectTitle(SyndicationItem candidate)
        {
            var id = UuidIri.Parse(candidate.Id);

            var expectedTitle = "Changeset " + (Guid)id;
            return candidate.Title != null
                && candidate.Title.Text == expectedTitle;
        }

        private bool HasCorrectLinks(SyndicationItem candidate)
        {
            var changesetId = UuidIri.Parse(candidate.Id);
            var expectedSelfUri = new Uri(
                ((Guid)changesetId).ToString(),
                UriKind.Relative);

            var expected = new HashSet<SyndicationLink>(
                this.Links,
                new SyndicationLinkComparer(expectedSelfUri));
            return expected.SetEquals(candidate.Links);
        }

        private class SyndicationLinkComparer :
            IEqualityComparer<SyndicationLink>
        {
            private readonly Uri expectedSelfUri;

            public SyndicationLinkComparer(Uri expectedSelfUri)
            {
                this.expectedSelfUri = expectedSelfUri;
            }

            public bool Equals(SyndicationLink x, SyndicationLink y)
            {
                if (x.RelationshipType == "self" && y.RelationshipType == "self")
                    return y.Uri == this.expectedSelfUri;
                if (x.RelationshipType == "via" && y.RelationshipType == "via")
                    return y.Uri == this.expectedSelfUri;
                return object.Equals(x.RelationshipType, y.RelationshipType)
                    && object.Equals(x.Uri, y.Uri);
            }

            public int GetHashCode(SyndicationLink obj)
            {
                return 0;
            }
        }

        private bool HasCorrectDates(SyndicationItem other)
        {
            return this.PublishDate <= other.PublishDate
                && other.PublishDate <= DateTimeOffset.Now
                && other.PublishDate == other.LastUpdatedTime;
        }

        private class SyndicationContentComparer :
            IEqualityComparer<SyndicationContent>
        {
            public bool Equals(SyndicationContent x, SyndicationContent y)
            {
                var atomX = ToAtomContent(x);
                var atomY = ToAtomContent(y);
                return XNode.DeepEquals(atomX, atomY);
            }

            public int GetHashCode(SyndicationContent obj)
            {
                return 0;
            }

            private static XNode ToAtomContent(SyndicationContent content)
            {
                var sb = new StringBuilder();
                using (var w = XmlWriter.Create(sb))
                {
                    content.WriteTo(w, "content", "http://www.w3.org/2005/Atom");
                    w.Flush();
                    return XElement.Parse(sb.ToString());
                }
            }
        }
    }
}
