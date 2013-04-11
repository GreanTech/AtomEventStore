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
        private readonly SyndicationFeed feed;

        public SyndicationFeedResemblance(SyndicationFeed feed)
        {
            this.feed = feed;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SyndicationFeed;
            if (other != null)
                return object.Equals(this.feed.Id, other.Id)
                    && this.HasCorrectLinks(other.Links)
                    && this.HasCorrectTitle(other.Title)
                    && HasCorrectAuthors(other.Authors);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        private bool HasCorrectLinks(IEnumerable<SyndicationLink> candidates)
        {
            var expected = new HashSet<SyndicationLink>(
                this.feed.Links,
                new SyndicationLinkComparer());
            return expected.SetEquals(candidates);
        }

        private class SyndicationLinkComparer :
            IEqualityComparer<SyndicationLink>
        {
            public bool Equals(SyndicationLink x, SyndicationLink y)
            {
                return object.Equals(x.RelationshipType, y.RelationshipType)
                    && object.Equals(x.Uri, y.Uri);
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
                    "Head of event stream " + this.feed.Id,
                    candidate.Text);
        }

        private static bool HasCorrectAuthors(
            IEnumerable<SyndicationPerson> candidate)
        {
            return candidate.Any(p => !string.IsNullOrWhiteSpace(p.Name));
        }
    }
}
