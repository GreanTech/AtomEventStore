using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomFeedParser<T> where T : IContentSerializer
    {
        private readonly T serializer;

        public AtomFeedParser(T serializer)
        {
            this.serializer = serializer;
        }

        public AtomFeed Parse(string xml)
        {
            return AtomFeed.Parse(xml, this.serializer);
        }
    }
}
