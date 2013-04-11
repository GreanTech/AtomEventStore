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
    public class SyndicationFeedResemblance : SyndicationFeed
    {
        public SyndicationFeedResemblance(SyndicationFeed feed)
            : base(feed, true)
        {
        }

        public override bool Equals(object obj)
        {
            var other = obj as SyndicationFeed;
            if (other != null)
                return object.Equals(this.Id, other.Id)
                    && this.HasCorrectLinks(other.Links)
                    && this.HasCorrectDate(other.LastUpdatedTime)
                    && this.HasCorrectTitle(other.Title)
                    && HasCorrectAuthors(other.Authors)
                    && this.HasCorrectItems(other.Items);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        private bool HasCorrectLinks(IEnumerable<SyndicationLink> candidates)
        {
            var expected = new HashSet<SyndicationLink>(
                this.Links,
                new SyndicationLinkComparer());
            return expected.SetEquals(candidates);
        }

        private class SyndicationLinkComparer :
            IEqualityComparer<SyndicationLink>
        {
            public bool Equals(SyndicationLink x, SyndicationLink y)
            {
                return (x.RelationshipType == "via"
                    && y.RelationshipType == "via")
                    || (object.Equals(x.RelationshipType, y.RelationshipType)
                    && object.Equals(x.Uri, y.Uri));
            }

            public int GetHashCode(SyndicationLink obj)
            {
                return 0;
            }
        }

        private bool HasCorrectTitle(TextSyndicationContent candidate)
        {
            return candidate != null
                && object.Equals(
                    "Head of event stream " + this.Id,
                    candidate.Text);
        }

        private bool HasCorrectDate(DateTimeOffset candidate)
        {
            return this.LastUpdatedTime <= candidate
                && candidate <= DateTimeOffset.Now;
        }

        private static bool HasCorrectAuthors(
            IEnumerable<SyndicationPerson> candidate)
        {
            return candidate.Any(p => !string.IsNullOrWhiteSpace(p.Name));
        }

        private bool HasCorrectItems(IEnumerable<SyndicationItem> candidates)
        {
            return this.Items.SequenceEqual(candidates);
        }
    }
}
