module EventFlow.Functional.FnAgg

    open EventFlow.Core
    open EventFlow.Aggregates
    open EventFlow.Functional
    open EventFlow.Functional.Handlers
    open EventFlow.Functional.Transitions

    type TransitionResult<'State> = 
        | Transition of 'State
        | Ignore
        | NoResult

    type private HandlerF<'Agg,'Id,'State,'Command,'Event when 'Agg :> FnAggregateRoot<'Agg,'Id,'State> and 'Id :> IIdentity> =
        'Agg -> 'State -> 'Command -> seq<'Event>

    type private TransitionF<'Agg,'Id,'State when 'Agg :> FnAggregateRoot<'Agg,'Id,'State> and 'Id :> IIdentity> = 
        'State -> IAggregateEvent<'Agg,'Id> -> TransitionResult<'State>

    type AggDefinition<'Agg,'Id,'State when 'Agg :> FnAggregateRoot<'Agg,'Id,'State> and 'Id :> IIdentity> = 
        private {
            initialState: unit -> 'State
            handlers: IHandler<'Agg,'Id,'State> list 
            transitions: Transition<'Agg,'Id,'State> list
        }

    let register options (definition: AggDefinition<'Agg,'Id,'State>) =

        let convertDefinition d = 
            new AggregateDefinition<'Agg,'Id,'State>(System.Func<'State>(d.initialState), d.handlers, d.transitions)

        FnAgg.RegisterAggregate(definition |> convertDefinition, options)

    let addTransition definition (f: TransitionF<'Agg,'Id,'State>) = 

            let convertTransition t = 
                match t with
                    | Transition state -> Transitions.TransitionResult.Transition(state)
                    | Ignore -> Transitions.TransitionResult.Ignore()
                    | NoResult -> Transitions.TransitionResult.NoResult()
            let transition = EventFlow.Functional.Transitions.Transition(fun s e -> f s e |> convertTransition)

            { definition with transitions = transition :: definition.transitions }

    let addHandler definition (f: HandlerF<'Agg,'Id,'State,'Command,'Event>) = 
            let handler = Handler(fun a s c -> f a s c |> Seq.cast) :> IHandler<'Agg,'Id,'State>
            { definition with handlers = handler :: definition.handlers }
       

    type ABuilder<'Agg,'Id,'State when 'Agg :> FnAggregateRoot<'Agg,'Id,'State> and 'Id :> IIdentity>() = 

        let createDefinition s = { AggDefinition.initialState = s; handlers = []; transitions = [] }
        let ignoreAggregate f = fun _ s c -> f s c
        let ignoreAggregateAndState f = fun _ _ c -> f c
        let singleEvent f = fun a s c -> f a s c |> Seq.singleton
        let optionalEvent f = fun a s c -> f a s c |> Option.toList |> List.toSeq
        let ignoreState f = fun _ e -> f e
        let implicitTransition f = fun s e -> f s e |> Transition
        let matchEvent f = fun s e ->
            match box e with
                | :? 'Event as e -> f s e
                | _ -> NoResult

        member __.Yield(_) = ()

        [<CustomOperation("initialState")>]
        member __.InitialState(_ : unit, f : unit -> 'State) : AggDefinition<'Agg,'Id,'State> = createDefinition f
    
        [<CustomOperation("handle")>]
        member __.Handle(d, f) = f |> addHandler d
        member __.Handle(d, f) = f |> singleEvent |> addHandler d
        member __.Handle(d, f) = f |> optionalEvent |> addHandler d
        member __.Handle(d, f) = f |> ignoreAggregate |> addHandler d
        member __.Handle(d, f) = f |> ignoreAggregate |> singleEvent |> addHandler d
        member __.Handle(d, f) = f |> ignoreAggregate |> optionalEvent |> addHandler d
        member __.Handle(d, f) = f |> ignoreAggregateAndState |> addHandler d
        member __.Handle(d, f) = f |> ignoreAggregateAndState |> singleEvent |> addHandler d
        member __.Handle(d, f) = f |> ignoreAggregateAndState |> optionalEvent |> addHandler d

        [<CustomOperation("transition")>]
        member __.Transition(d, f) = f |> addTransition d
        member __.Transition(d, f) = f |> matchEvent |> addTransition d
        member __.Transition(d, f) = f |> implicitTransition |> addTransition d
        member __.Transition(d, f) = f |> implicitTransition |> matchEvent |> addTransition d
        member __.Transition(d, f) = f |> ignoreState |> addTransition d
        member __.Transition(d, f) = f |> ignoreState |> matchEvent |> addTransition d
        member __.Transition(d, f) = f |> ignoreState |> implicitTransition |> addTransition d
        member __.Transition(d, f) = f |> ignoreState |> implicitTransition |> matchEvent |> addTransition d

    let build () = ABuilder<'Agg,'Id,'State>()
