using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq.Language.Flow;

namespace Grean.AtomEventStore.UnitTests
{
    public static class MockEnvy
    {
        public static ICallbackResult InSequence<T>(
            this ISetup<T> setup,
            SpySequence sequence) where T : class
        {
            var i = sequence.GetNextExpectedIndex();
            return setup.Callback(() => sequence.Register(i));
        }
    }
}
