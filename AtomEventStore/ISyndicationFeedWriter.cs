using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore
{
    public interface ISyndicationFeedWriter
    {
        void CreateOrUpdate(SyndicationFeed feed);
    }
}
