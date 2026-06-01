# AtomEventStore

A server-less .NET Event Store based on the Atom syndication format.

## Highlights:

AtomEventStore is designed to be a lightweight Event Store implementation. It offers the following benefits:

- No server required; only storage
- Human-readable storage format
- Supports file storage
- Supports in-memory storage
- Supports Azure Blob storage
- Designed to be [scalable](https://github.com/GreanTech/AtomEventStore/wiki/Scalable)
- [Extensible](https://github.com/GreanTech/AtomEventStore/wiki/Reusable)

Due to its flexible design and simple storage requirements, you can easily implement a storage implementation on top of your favourite storage mechanism: document databases, relational database, and so on. All you have to do is to implement a *single interface with two methods!* If you are interested in contributing a storage implementation, please first [submit an issue](https://github.com/GreanTech/AtomEventStore/issues) to start the discussion.

## At a glance
AtomEventStore is easy to use, and is built on well-known abstractions already present in .NET.

### Writing events
You [write events](https://github.com/GreanTech/AtomEventStore/wiki/Writing-events) one at a time using the `AtomEventObserver<T>` class.

#### Synchronous writes

In this example, `obs` is an instance of `AtomEventObserver<object>`, and `userCreated` is represent an event: 

```C#
obs.OnNext(userCreated);
```

When the call to `obs.OnNext` returns, the `userCreated` event has been written to `storage`.

#### Asynchronous writes

As of 2.0, genuinely asynchronous writes are available through `AsyncAtomEventObserver<T>`, backed by an `IAsyncAtomEventStorage`. The built-in `AtomEventsInMemory` and `AtomEventsInFiles` implement both the synchronous and asynchronous storage interfaces, so the same storage works either way:

```C#
var obs = new AsyncAtomEventObserver<object>(id, pageSize, storage, serializer);
await obs.AppendAsync(userCreated);
```

When the task completes, the `userCreated` event has been written to `storage`.

> The older `AtomEventObserver<T>.AppendAsync` is now `[Obsolete]` (a warning): it offloaded *synchronous* I/O to the thread pool rather than performing real asynchronous I/O. Prefer `AsyncAtomEventObserver<T>`. The synchronous `AtomEventObserver<T>.OnNext` is unchanged.

### Reading events

You can [read events](https://github.com/GreanTech/AtomEventStore/wiki/Reading-events) either forwards or backwards. In both cases, reading events is based on the standard `IEnumerable<T>` interface.

```C#
var firstEvent = events.First();
```

The above example simply reads the first event in the event stream.  

#### Asynchronous reads

As of 2.0 you can also read asynchronously via `AsyncFifoEvents<T>` and `AsyncLifoEvents<T>`, which implement `IAsyncEnumerable<T>`:

```C#
var events = new AsyncFifoEvents<object>(id, storage, serializer);
await foreach (var e in events)
{
    // handle each event
}
```

## Migrating to 2.0

Version 2.0 modernises AtomEventStore for .NET Core / modern .NET while keeping the existing **synchronous API source-compatible**. Code that uses `AtomEventObserver<T>.OnNext`, `FifoEvents<T>`, `LifoEvents<T>`, `AtomEventsInMemory`, or `AtomEventsInFiles` continues to work unchanged.

- **Target framework.** The library now targets **.NET Standard 2.0** (previously .NET Framework 4.5), so it is consumable from .NET Framework 4.6.1+, .NET Core, and modern .NET. *The one consumer-visible compatibility change:* projects still on .NET Framework **earlier than 4.6.1** must raise their target framework to use 2.0.
- **New asynchronous API (additive).** Alongside the unchanged synchronous types, 2.0 adds `IAsyncAtomEventStorage`, `AsyncAtomEventObserver<T>`, and `AsyncFifoEvents<T>` / `AsyncLifoEvents<T>`. The asynchrony is at the I/O boundary; serialization stays synchronous (the underlying serializers expose no async API).
- **Deprecation.** `AtomEventObserver<T>.AppendAsync` is now `[Obsolete]` (a warning, not an error) because it was never genuinely asynchronous. Migrate to `AsyncAtomEventObserver<T>`, or keep using synchronous `OnNext`.
- **New dependency.** The package now depends on `Microsoft.Bcl.AsyncInterfaces` (which supplies `IAsyncEnumerable<T>` on .NET Standard 2.0).
- **Azure Blob storage.** The published `AtomEventStore.AzureBlob` package (synchronous, on the legacy `WindowsAzure.Storage` SDK) is **frozen** and remains available for existing consumers. For modern asynchronous Azure Blob storage on **`Azure.Storage.Blobs` (v12)**, use the `AtomEventStore.AzureBlobAsync` **reference implementation** in this repository — it implements `IAsyncAtomEventStorage` and is meant to be copied or adapted rather than consumed as a separately published package.

## NuGet

AtomEventStore is available via NuGet:

- [AtomEventStore](http://www.nuget.org/packages/AtomEventStore/) — 2.0+ targets .NET Standard 2.0.
- [AtomEventStore.AzureBlob](http://www.nuget.org/packages/AtomEventStore.AzureBlob/) — legacy, synchronous, .NET Framework; frozen (see *Migrating to 2.0* above).

## Versioning

AtomEventStore follows [Semantic Versioning 2.0.0](http://semver.org/spec/v2.0.0.html).

## Credits

The idea that an Event Store can be modelled as a Linked List was originally put to my attention by [Yves Reynhout](http://seabites.wordpress.com) in his article [Your EventStream is a linked list](http://bit.ly/AqearV).
