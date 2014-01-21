using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;

namespace Grean.AtomEventStore.UnitTests
{
    public class AutoAtomDataAttribute : AutoDataAttribute
    {
        public AutoAtomDataAttribute()
            : base(new Fixture().Customize(new AtomEventsCustomization()))
        {
        }
    }
}
