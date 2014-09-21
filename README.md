# AtomEventStore

A server-less .NET Event Store based on the Atom syndication format.

## Highlights:

- No server required; only storage
- Human-readable storage format
- Supports file storage
- Supports in-memory storage
- Supports Azure Blob storage
- Designed to be scalable
- Extensible

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

Asynchronous writes can be done using the standard Task Parallel Library (TPL) model for asynchrony:

```C#
await obs.AppendAsync(userCreated);
```

Notice that since `AtomEventObserver<T>` uses the standard TPL model, you can use it with `async` and `await`.

When the task returned by `obs.AppendAsync` completes, the `userCreated` event has been written to `storage`.

### Reading events

You can read events either forwards or backwards. In both cases, reading events is based on the standard `IEnumerable<T>` interface.

#### Reading in original order

If you want to read the events in the order they were written, with the oldest event first, you should use the *First-In, First-Out* reader:

```C#
IEnumerable<object> events = new FifoEvents<object>(
    eventStreamId, // a Guid
    storage,       // an IAtomEventStorage object
    serializer);   // an IContentSerializer object
var firstEvent = events.First();
```

It's not necessary to explicitly declare `events` as `IEnumerable<object>`: you can use the `var` keyword as well; this example just uses explicit variable declaration in order to make it clearer what's going on.

#### Reading in reverse order

If you want to read the most recent events first, you can use the `LifoEvents<T>` class instead of `FifoEvents<T>`: it provides a *Last-In, First-Out* Iterator over the event stream.

#### Filtering, aggregation, and projections

Since both `FifoEvents<T>` and `LifoEvents<T>` implement `IEnumerable<T>`, you can perform *any* filtering, aggregation, and projection operation you're used to be able to do with LINQ. However, be aware that there's no protocol translation going on (`IQueryable<T>` is not in use). All operations on the event stream happen on the Iterator in memory, so if you're not careful, you may inadvertently read the entire event stream into memory from storage.

However, with a bit of care, and judicious selection of `FifoEvents<T>` or `LifoEvents<T>` you can still make your system efficient.  

## NuGet

AtomEventStore is available via NuGet:

- [AtomEventStore](http://www.nuget.org/packages/AtomEventStore/)
- [AtomEventStore.AzureBlob](http://www.nuget.org/packages/AtomEventStore.AzureBlob/)

## Versioning

AtomEventStore follows [Semantic Versioning 2.0.0](http://semver.org/spec/v2.0.0.html).

## Credits

The idea that an Event Store can be modelled as a Linked List was originally put to my attention by [Yves Reynhout](http://seabites.wordpress.com) in his article [Your EventStream is a linked list](http://bit.ly/AqearV).
