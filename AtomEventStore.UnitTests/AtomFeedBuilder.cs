using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomFeedBuilder<T>
    {
        private readonly AtomFeed feed;
        private readonly IEnumerable<AtomEntryBuilder<T>> entryBuilders;

        public AtomFeedBuilder(
            AtomFeed feed, 
            IEnumerable<AtomEntryBuilder<T>> entryBuilders)
        {
            this.feed = feed;
            this.entryBuilders = entryBuilders;
        }

        public AtomFeed Build()
        {
            return this.feed
                .WithEntries(this.entryBuilders.Select(b => b.Build()))
                .AddLink(AtomLink.CreateSelfLink(
                    new Uri(
                        ((Guid)this.feed.Id).ToString(),
                        UriKind.Relative)));
        }

        public static implicit operator AtomFeed(AtomFeedBuilder<T> builder)
        {
            return builder.Build();
        }
    }
}
