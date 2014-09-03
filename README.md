# AtomEventStore

A server-less .NET Event Store based on the Atom syndication format.

## Highlights:

- No server required; only storage
- Supports file storage
- Supports in-memory storage
- Supports Azure Blob storage
- Designed to be scalable
- Extensible

Due to its flexible design and simple storage requirements, you can easily implement a storage implementation on top of your favourite storage mechanism: document databases, relational database, and so on. All you have to do is to implement a *single interface with two methods!* If you are interested in contributing a storage implementation, please first [submit an issue](https://github.com/GreanTech/AtomEventStore/issues) to start the discussion. 

## NuGet

AtomEventStore is available via NuGet:

- [AtomEventStore](http://www.nuget.org/packages/AtomEventStore/)
- [AtomEventStore.AzureBlob](http://www.nuget.org/packages/AtomEventStore.AzureBlob/)

## Versioning

AtomEventStore follows [Semantic Versioning 2.0.0](http://semver.org/spec/v2.0.0.html).

## Credits

The idea that an Event Store can be modelled as a Linked List was originally put to my attention by [Yves Reynhout](http://seabites.wordpress.com) in his article [Your EventStream is a linked list](http://bit.ly/AqearV).
