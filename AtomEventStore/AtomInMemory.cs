using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Grean.AtomEventStore
{
    public class AtomInMemory
    {
        private StringBuilder entry;

        public AtomInMemory()
        {
            this.entry = new StringBuilder();
        }

        public XmlWriter CreateWriterFor(AtomEntry atomEntry)
        {
            return XmlWriter.Create(entry);
        }

        public XmlReader CreateReaderFor(UuidIri id)
        {
            var sr = new StringReader(this.entry.ToString());
            return XmlReader.Create(
                sr,
                new XmlReaderSettings { CloseInput = true });
        }
    }
}
