using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Grean.AtomEventStore;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using Ploeh.AutoFixture;

namespace Grean.AtomEventStore.UnitTests
{
    public class FifoEventsTests
    {
        [Theory, AutoAtomData]
        public void SutIsEnumerable(FifoEvents<XmlAttributedTestEventX> sut)
        {
            Assert.IsAssignableFrom<IEnumerable<XmlAttributedTestEventX>>(sut);
        }

        [Theory, AutoAtomData]
        public void PropertiesAreCorrectlyInitialized(
            ConstructorInitializedMemberAssertion assertion)
        {
            assertion.Verify(typeof(FifoEvents<XmlAttributedTestEventX>));
        }

        [Theory, AutoAtomData]
        public void SutIsInitiallyEmpty(
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            FifoEvents<XmlAttributedTestEventX> sut)
        {
            Assert.False(sut.Any(), "Intial event stream should be empty.");
            Assert.Empty(sut);
        }

        [Theory, AutoAtomData]
        public void SutYieldsCorrectEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            [Frozen]UuidIri dummyId,
            AtomEventObserver<XmlAttributedTestEventX> writer,
            FifoEvents<XmlAttributedTestEventX> sut,
            List<XmlAttributedTestEventX> expected)
        {
            expected.ForEach(e => writer.AppendAsync(e).Wait());

            Assert.True(
                expected.SequenceEqual(sut),
                "Events should be yielded in a FIFO order");
            Assert.True(
                expected.Cast<object>().SequenceEqual(sut.OfType<object>()),
                "Events should be yielded in a FIFO order");
        }

        [Theory, AutoAtomData]
        public void SutYieldsPagedEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            [Frozen]UuidIri dummyId,
            AtomEventObserver<XmlAttributedTestEventX> writer,
            FifoEvents<XmlAttributedTestEventX> sut,
            Generator<XmlAttributedTestEventX> eventGenerator)
        {
            var expected = eventGenerator.Take(writer.PageSize * 2 + 1).ToList();

            expected.ForEach(e => writer.AppendAsync(e).Wait());

            Assert.True(
                expected.SequenceEqual(sut),
                "Events should be yielded in a FIFO order");
            Assert.True(
                expected.Cast<object>().SequenceEqual(sut.OfType<object>()),
                "Events should be yielded in a FIFO order");
        }

        [Theory, AutoAtomData]
        public void SutCanAppendAndYieldPolymorphicEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            [Frozen]UuidIri dummyId,
            AtomEventObserver<IXmlAttributedTestEvent> writer,
            FifoEvents<IXmlAttributedTestEvent> sut,
            XmlAttributedTestEventX tex,
            XmlAttributedTestEventY tey)
        {
            writer.AppendAsync(tex).Wait();
            writer.AppendAsync(tey).Wait();

            var expected = new IXmlAttributedTestEvent[] { tex, tey };
            Assert.True(expected.SequenceEqual(sut));
        }

        [Theory, AutoAtomData]
        public void ReverseYieldsCorrectEvents(
            [Frozen(As = typeof(ITypeResolver))]TestEventTypeResolver dummyResolver,
            [Frozen(As = typeof(IContentSerializer))]XmlContentSerializer dummySerializer,
            [Frozen(As = typeof(IAtomEventStorage))]AtomEventsInMemory dummyInjectedIntoSut,
            [Frozen]UuidIri dummyId,
            AtomEventObserver<XmlAttributedTestEventX> writer,
            FifoEvents<XmlAttributedTestEventX> sut,
            List<XmlAttributedTestEventX> expected)
        {
            Enumerable
                .Reverse(expected)
                .ToList()
                .ForEach(e => writer.AppendAsync(e).Wait());

            var actual = sut.Reverse();

            Assert.True(
                expected.SequenceEqual(actual),
                "Events should be yielded in a LIFO order");
            Assert.True(
                expected.Cast<object>().SequenceEqual(actual.OfType<object>()),
                "Events should be yielded in a LIFO order");
        }

        [Theory, AutoAtomData]
        public void ReverseReturnsCorrectResult(
            UuidIri id,
            AtomEventsInMemory storage,
            XmlContentSerializer serializer)
        {
            var sut =
                new FifoEvents<XmlAttributedTestEventX>(id, storage, serializer);
            var expected =
                new LifoEvents<XmlAttributedTestEventX>(id, storage, serializer);

            var actual = sut.Reverse();

            var lifo = Assert.IsType<LifoEvents<XmlAttributedTestEventX>>(actual);
            Assert.Equal(expected.Id, lifo.Id);
            Assert.Equal(expected.Storage, lifo.Storage);
            Assert.Equal(expected.Serializer, lifo.Serializer);
        }
    }
}
