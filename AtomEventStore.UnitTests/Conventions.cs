using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Idioms;
using Xunit.Extensions;

namespace Grean.AtomEventStore.UnitTests
{
    public class Conventions
    {
        [Theory, AutoAtomData]
        public void VerifyGuardClauses(GuardClauseAssertion assertion)
        {
            var representative = typeof(AtomEventStream);
            var targets = from t in representative.Assembly.GetExportedTypes()
                          where Include(t)
                          from m in t.GetMembers()
                          where m.Name != "TryParse"
                          select m;

            assertion.Verify(targets);
        }

        private static bool Include(Type t)
        {
            return !t.IsInterface
                && !t.IsGenericTypeDefinition
                && !IsStatic(t)
                && t != typeof(AtomLink); // Covered by AtomLinkTests
        }

        private static bool IsStatic(Type t)
        {
            return t.IsSealed && t.IsAbstract;
        }
    }
}
