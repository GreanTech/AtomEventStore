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

Asynchronous writes can be done using the standard Task Parallel Library (TPL) model for asynchrony:

```C#
await obs.AppendAsync(userCreated);
```

Notice that since `AtomEventObserver<T>` uses the standard TPL model, you can use it with `async` and `await`.

When the task returned by `obs.AppendAsync` completes, the `userCreated` event has been written to `storage`.

### Reading events

You can [read events](https://github.com/GreanTech/AtomEventStore/wiki/Reading-events) either forwards or backwards. In both cases, reading events is based on the standard `IEnumerable<T>` interface.

```C#
var firstEvent = events.First();
```

The above example simply reads the first event in the event stream.  

## NuGet

AtomEventStore is available via NuGet:

- [AtomEventStore](http://www.nuget.org/packages/AtomEventStore/)
- [AtomEventStore.AzureBlob](http://www.nuget.org/packages/AtomEventStore.AzureBlob/)

## Versioning

AtomEventStore follows [Semantic Versioning 2.0.0](http://semver.org/spec/v2.0.0.html).

## Credits

The idea that an Event Store can be modelled as a Linked List was originally put to my attention by [Yves Reynhout](http://seabites.wordpress.com) in his article [Your EventStream is a linked list](http://bit.ly/AqearV).
