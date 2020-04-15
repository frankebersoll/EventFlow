namespace Tests

module Documents =

    open NUnit.Framework
    open EventFlow.Core
    open EventFlow.Functional
    open EventFlow.Aggregates
    open EventFlow.Extensions
    open EventFlow
    open EventFlow.ValueObjects
    open EventFlow.Commands

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
        member x.metadata = metadata
    
    type AggregateTests () =

        let createDocument (c: CreateDocument) = { DocumentCreated.metadata = c.metadata }

        let onDocumentCreated (e: DocumentCreated) = DocumentState.Active e.metadata

        [<SetUp>]
        member this.Setup () = ()

        [<Test>]
        member this.Test1 () =
                            
            let customerDefinition = FnAgg.build() { 

                initialState (fun () -> DocumentState.Initial)

                handle createDocument 
                transition onDocumentCreated
            }

            let o = EventFlowOptions.New.AddDefaults(typeof<AggregateTests>.Assembly)
            FnAgg.register o customerDefinition

            let bus = o.CreateResolver().Resolve<ICommandBus>()

            let command = CreateDocument(DocumentId.New, { Metadata.title = Title("asdf") })

            bus.Publish(command) |> ignore

            Assert.Pass()
