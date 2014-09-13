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
You write events one at a time using the `AtomEventObserver<T>` class.

#### Synchronous writes

`AtomEventObserver<T>` supports both synchronous and asynchronous writes. Synchronous writes offer the advantage that you can treat `AtomEventObserver<T>` as an `IObserver<T>`: 

```C#
IObserver<object> obs = new AtomEventObserver<object>(
    eventStreamId, // a Guid
    pageSize,      // an Int32
    storage,       // an IAtomEventStorage object
    serializer);   // an IContentSerializer object

var userCreated = new UserCreated
{
    UserId = eventStreamId,
    UserName = "ploeh",
    Password = "12345",
    Email = "ploeh@fnaah.com"
};
obs.OnNext(userCreated);
```

It's not necessary to explicitly declare `obs` as `IObserver<object>`: you can use the `var` keyword as well; this example just uses explicit variable declaration in order to make it clearer what's going on.

When the call to `obs.OnNext` returns, the `userCreated` event has been written to `storage`.

#### Asynchronous writes

Asynchronous writes can be done using the standard Task Parallel Library (TPL) model for asynchrony:

```C#
var obs = new AtomEventObserver<object>(
    eventStreamId, // a Guid
    pageSize,      // an Int32
    storage,       // an IAtomEventStorage object
    serializer);   // an IContentSerializer object

var userCreated = new UserCreated
{
    UserId = eventStreamId,
    UserName = "ploeh",
    Password = "12345",
    Email = "ploeh@fnaah.com"
};
await obs.AppendAsync(userCreated);
```

Notice that since `AtomEventObserver<T>` uses the standard TPL model, you can use it with `async` and `await`.

when the task returned by `obs.AppendAsync` completes, the `userCreated` event has been written to `storage`.

## NuGet

AtomEventStore is available via NuGet:

- [AtomEventStore](http://www.nuget.org/packages/AtomEventStore/)
- [AtomEventStore.AzureBlob](http://www.nuget.org/packages/AtomEventStore.AzureBlob/)

## Versioning

AtomEventStore follows [Semantic Versioning 2.0.0](http://semver.org/spec/v2.0.0.html).

## Credits

The idea that an Event Store can be modelled as a Linked List was originally put to my attention by [Yves Reynhout](http://seabites.wordpress.com) in his article [Your EventStream is a linked list](http://bit.ly/AqearV).
