namespace Grean.AtomEventStore.UnitTests.FSharp.Demo.UserSignUp

open System
open System.Runtime.Serialization
open System.Reflection
open Microsoft.FSharp.Reflection

[<CLIMutable>]
type UserCreated = {
    UserId : Guid
    UserName : string
    Password : string
    Email : string }

[<CLIMutable>]
type EmailVerified = {
    UserId : Guid
    Email : string }

[<CLIMutable>]
type EmailChanged = {
    UserId : Guid
    NewEmail : string }

type UserEvent =
    | UserCreated of UserCreated
    | EmailVerified of EmailVerified
    | EmailChanged of EmailChanged

type User = {
    Id : Guid
    Name : string
    Password : string
    Email : string
    EmailVerified : bool }

module UserEvents =
    let createUser (uc : UserCreated) =
        {
            Id = uc.UserId
            Name = uc.UserName
            Password = uc.Password
            Email = uc.Email
            EmailVerified = false
        }

    let verifyEmail u = { u with EmailVerified = true }

    let changeEmail ec u = { u with Email = ec.NewEmail; EmailVerified = false }

    let applyEvent users e =
        let findUser id = users |> Seq.find (fun x -> x.Id = id)
        let updateUser u =
            users
            |> Seq.filter (fun x -> x.Id <> u.Id)
            |> Seq.append [u]
        match e with
        | UserCreated uc -> users |> Seq.append [createUser uc]
        | EmailVerified ev -> findUser ev.UserId |> verifyEmail |> updateUser
        | EmailChanged ec -> findUser ec.UserId |> changeEmail ec |> updateUser

    let foldEvents events =
        events |> Seq.fold applyEvent Seq.empty

module Stories =
    open Xunit
    open Grean.AtomEventStore
    open Grean.AtomEventStore.UnitTests.FSharp.TestDsl

    let resolver =
        { new ITypeResolver with
            member this.Resolve(localName, xmlNamespace) =
                match (localName, xmlNamespace) with
                | ("UserEvent.UserCreated", "http://schemas.datacontract.org/2004/07/Grean.AtomEventStore.UnitTests.FSharp.Demo.UserSignUp") ->
                    typeof<UserEvent>.GetNestedType("UserCreated")
                | ("UserEvent.EmailVerified", "http://schemas.datacontract.org/2004/07/Grean.AtomEventStore.UnitTests.FSharp.Demo.UserSignUp") ->
                    typeof<UserEvent>.GetNestedType("EmailVerified")
                | ("UserEvent.EmailChanged", "http://schemas.datacontract.org/2004/07/Grean.AtomEventStore.UnitTests.FSharp.Demo.UserSignUp") ->
                    typeof<UserEvent>.GetNestedType("EmailChanged")
                | _ -> invalidArg "localName or xmlNamespace" (sprintf """Invalid local name "%s" or XML namespace "%s".""" localName xmlNamespace) }

    [<Fact>]
    let ``Write a single event synchronously`` () =
        let eventStreamId = 
            Guid "1B858B8F-A29E-4C27-8880-5746584088EE" |> UuidIri.op_Implicit
        use storage = new AtomEventsInMemory()
        let pageSize = 25
        let serializer = DataContractContentSerializer resolver
        let obs =
            AtomEventObserver<UserEvent>(
                eventStreamId, // an UuidIri (basically, a Guid)
                pageSize,      // an Int32
                storage,       // an IAtomEventStorage object
                serializer)    // an IContentSerializer object
        
        let userCreated = UserCreated {
            UserId = eventStreamId |> UuidIri.op_Implicit
            UserName = "ploeh"
            Password = "12345"
            Email = "ploeh@fnaah.com" }
        obs.OnNext userCreated
        
        Verify <@ not (storage |> Seq.isEmpty) @>

    let appendAsync (obs : AtomEventObserver<_>) x =
        obs.AppendAsync x |> Async.AwaitIAsyncResult |> Async.Ignore

    [<Fact>]
    let ``Write a single event asynchronously`` () =
        let eventStreamId = 
            Guid "1B858B8F-A29E-4C27-8880-5746584088EE" |> UuidIri.op_Implicit
        use storage = new AtomEventsInMemory()
        let pageSize = 25
        let serializer = DataContractContentSerializer resolver
        let obs =
            AtomEventObserver<UserEvent>(
                eventStreamId, // an UuidIri (basically, a Guid)
                pageSize,      // an Int32
                storage,       // an IAtomEventStorage object
                serializer)    // an IContentSerializer object
        
        let userCreated = UserCreated {
            UserId = eventStreamId |> UuidIri.op_Implicit
            UserName = "ploeh"
            Password = "12345"
            Email = "ploeh@fnaah.com" }
        userCreated |> appendAsync obs |> Async.RunSynchronously
        
        Verify <@ not (storage |> Seq.isEmpty) @>

    [<Fact>]
    let ``Read multiple events`` () =
        let eventStreamId = 
            Guid "1B858B8F-A29E-4C27-8880-5746584088EE" |> UuidIri.op_Implicit
        use storage = new AtomEventsInMemory()
        let pageSize = 25
        let serializer = DataContractContentSerializer resolver
        let obs =
            AtomEventObserver<UserEvent>(
                eventStreamId, // an UuidIri (basically, a Guid)
                pageSize,      // an Int32
                storage,       // an IAtomEventStorage object
                serializer)    // an IContentSerializer object
        obs.OnNext(UserCreated {
            UserId = eventStreamId |> UuidIri.op_Implicit
            UserName = "ploeh"
            Password = "12345"
            Email = "ploeh@fnaah.dk" })
        obs.OnNext(EmailVerified {
            UserId = eventStreamId |> UuidIri.op_Implicit
            Email = "ploeh@fnaah.dk" })
        obs.OnNext(EmailChanged {
            UserId = eventStreamId |> UuidIri.op_Implicit
            NewEmail = "fnaah@ploeh.dk" })

        let events =
            FifoEvents<UserEvent>(
                eventStreamId, // an UuidIri (basically, a Guid)
                storage,       // an IAtomEventStorage object
                serializer)    // an IContentSerializer object
        let users = events |> UserEvents.foldEvents
    
        let expected = {
            Id = eventStreamId |> UuidIri.op_Implicit
            Name = "ploeh"
            Password = "12345"
            Email = "fnaah@ploeh.dk"
            EmailVerified = false }
        Verify <@ users |> Seq.filter ((=) expected) |> Seq.length = 1 @>