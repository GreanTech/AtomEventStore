using System.ServiceModel.Syndication;

namespace Grean.AtomEventStore
{
    public interface ISyndicationItemReader
    {
        SyndicationItem ReadItem(string id);
    }
}
