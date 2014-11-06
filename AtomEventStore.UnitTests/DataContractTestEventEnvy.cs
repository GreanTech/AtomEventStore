using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grean.AtomEventStore.UnitTests
{
    internal static class DataContractTestEventEnvy
    {
        internal static string AsSerializedString(
            this IDataContractTestEvent @event,
            IContentSerializer serializer)
        {
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                serializer.Serialize(w, @event);
                w.Flush();
            }
            return sb.ToString();
        }
    }
}
