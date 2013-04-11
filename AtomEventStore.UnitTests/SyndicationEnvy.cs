using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public static class SyndicationEnvy
    {
        public static SyndicationItemResemblance ToResemblance(
            this SyndicationItem syndicationItem)
        {
            return new SyndicationItemResemblance(syndicationItem);
        }

        public static SyndicationFeedResemblance ToResemblance(
            this SyndicationFeed syndicationFeed)
        {
            return new SyndicationFeedResemblance(syndicationFeed);
        }

        public static SyndicationItem ChangeLinkRelationShipTypes(
            this SyndicationItem syndicationItem,
            string from,
            string to)
        {
            var newItem = syndicationItem.Clone();
            foreach (var l in newItem.Links.Where(l => l.RelationshipType == from))
                l.RelationshipType = to;
            return newItem;
        }
    }
}
