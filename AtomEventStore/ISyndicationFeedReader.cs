using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;

namespace Grean.AtomEventStore
{
    public interface ISyndicationFeedReader
    {
        SyndicationFeed Read(string id);
    }
}
