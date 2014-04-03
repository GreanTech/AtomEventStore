using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grean.AtomEventStore.UnitTests
{
    public static class Disarray
    {
        public static T PickRandom<T>(this IReadOnlyCollection<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(
            this IReadOnlyCollection<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(
            this IReadOnlyCollection<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}
