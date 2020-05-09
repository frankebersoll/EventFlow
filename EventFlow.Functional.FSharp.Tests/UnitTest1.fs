namespace EventFlow.Functional.FSharp.Tests

open NUnit.Framework
open EventFlow.Core
open EventFlow.Functional
open EventFlow.Aggregates
open EventFlow.Extensions
open EventFlow
open EventFlow.ValueObjects
open EventFlow.Commands

module Domain =

    type DocumentId(id) = inherit Identity<DocumentId>(id)
    
    type Title(s) = inherit SingleValueObject<string>(s)
    type Metadata = { title: Title }

    type DocumentState =
        | Initial
        | Active of Metadata
        | Approved of Metadata
        | Deleted
    
    [<AggregateName("Document")>]
    type DocumentAggregate(id, state) = inherit FnAggregateRoot<DocumentAggregate, DocumentId, DocumentState>(id, state)

    type DocumentEvent = IAggregateEvent<DocumentAggregate, DocumentId>

    type DocumentCreated = { metadata: Metadata } interface DocumentEvent
    type DocumentRenamed = { title: Title } interface DocumentEvent

    type DocumentCommand = Command<DocumentAggregate, DocumentId>

    type CreateDocument(id, metadata) = inherit DocumentCommand(id) with
        member this.metadata = metadata

    let private createDocument (c: CreateDocument) = { DocumentCreated.metadata = c.metadata }
    let private onDocumentCreated (e: DocumentCreated) = DocumentState.Active e.metadata

    let documentAggregate: FnAgg.AggDefinition<DocumentAggregate, DocumentId, DocumentState> = 
        FnAgg.build() { 
            initialState (fun () -> DocumentState.Initial)
            handle createDocument 
            transition onDocumentCreated
        }

open Domain
open EventFlow.Configuration
open EventFlow.EventStores
open FsUnit
    
type FSharpAggregateTests () =

    let getEvents (resolver: IResolver) =
        let persistence = resolver.Resolve<IEventStore>()
        persistence.LoadAllEvents(GlobalPosition.Start, 100).DomainEvents

    [<SetUp>]
    member this.Setup () = ()

    [<Test>]
    member this.SimpleAggregateTest () =
                            
        let options = EventFlowOptions.New.AddDefaults(typeof<FSharpAggregateTests>.Assembly)
        FnAgg.register options documentAggregate
        let resolver = options.CreateResolver()
        let bus = resolver.Resolve<ICommandBus>()

        let command = CreateDocument(DocumentId.New, { Metadata.title = Title("asdf") })

        bus.Publish(command) |> ignore

        let event = resolver |> getEvents
        event |> should haveCount 1

