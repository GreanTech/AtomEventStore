using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public class AtomEntryBuilder<T>
    {
        private readonly AtomEntry entry;
        private readonly T item;

        public AtomEntryBuilder(AtomEntry entry, T item)
        {
            this.entry = entry;
            this.item = item;
        }

        public AtomEntry Build()
        {
            return this.entry
                .WithContent(this.entry.Content.WithItem(this.item))
                .AddLink(AtomLink.CreateSelfLink(
                    new Uri(
                        ((Guid)this.entry.Id).ToString(),
                        UriKind.Relative)));
        }

        public static implicit operator AtomEntry(AtomEntryBuilder<T> builder)
        {
            return builder.Build();
        }
    }
}
