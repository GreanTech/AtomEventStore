﻿using System;
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
                    && HasCorrectLinks(other)
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

        private static bool HasCorrectLinks(SyndicationItem candidate)
        {
            var changesetId = UuidIri.Parse(candidate.Id);
            var expectedUri =
                new Uri(((Guid)changesetId).ToString(), UriKind.Relative);
            Func<SyndicationLink, bool> isCorrectSelfLink = l =>
                l.RelationshipType == "self" &&
                l.Uri == expectedUri;

            return candidate != null
                && candidate.Links.Count(isCorrectSelfLink) == 1;
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
