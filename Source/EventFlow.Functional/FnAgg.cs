using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.Functional.Extensions;
using EventFlow.Functional.Handlers;
using EventFlow.Functional.Transitions;

namespace EventFlow.Functional
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class FnAgg<TAgg, TId, TState>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        public static Handler<TAgg, TId, TState, TCommand> Handle<TCommand>(
            Func<TAgg, TState, TCommand, IEnumerable<IAggregateEvent<TAgg, TId>>> f)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            return new Handler<TAgg, TId, TState, TCommand>(f);
        }

        public static Handler<TAgg, TId, TState, TCommand> Handle<TCommand>(
            Func<TAgg, TState, TCommand, IAggregateEvent<TAgg, TId>> f)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            return Handle(f
                .ToEnumerable());
        }

        public static Handler<TAgg, TId, TState, TCommand> Handle<TCommand>(
            Func<TState, TCommand, IEnumerable<IAggregateEvent<TAgg, TId>>> f)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            return Handle(f
                .IgnoreInput(default(TAgg)));
        }

        public static Handler<TAgg, TId, TState, TCommand> Handle<TCommand>(
            Func<TState, TCommand, IAggregateEvent<TAgg, TId>> f)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            return Handle(f
                .IgnoreInput(default(TAgg))
                .ToEnumerable());
        }

        public static Handler<TAgg, TId, TState, TCommand> Handle<TCommand>(
            Func<TCommand, IAggregateEvent<TAgg, TId>> f)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            return Handle(f
                .IgnoreInput(default(TState))
                .IgnoreInput(default(TAgg))
                .ToEnumerable());
        }

        public static Handler<TAgg, TId, TState, TCommand> Handle<TCommand>(
            Func<TCommand, IEnumerable<IAggregateEvent<TAgg, TId>>> f)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            return Handle(f
                .IgnoreInput(default(TState))
                .IgnoreInput(default(TAgg)));
        }

        public static Transition<TAgg, TId, TState> Transition(
            Func<TState, IAggregateEvent<TAgg, TId>, TransitionResult<TState>> f)
        {
            return new Transition<TAgg, TId, TState>(f);
        }

        public static Transition<TAgg, TId, TState> Transition<TEvent>(Func<TState, TEvent, TransitionResult<TState>> f)
            where TEvent : IAggregateEvent<TAgg, TId>
        {
            return Transition(f
                .MatchEvent<TAgg, TId, TState, TEvent>());
        }

        public static Transition<TAgg, TId, TState> Transition(Func<IAggregateEvent<TAgg, TId>, TransitionResult<TState>> f)
        {
            return Transition(f
                .IgnoreInput(default(TState)));
        }

        public static Transition<TAgg, TId, TState> Transition<TEvent>(Func<TEvent, TransitionResult<TState>> f)
            where TEvent : IAggregateEvent<TAgg, TId>
        {
            return Transition(f
                .IgnoreInput(default(TState))
                .MatchEvent<TAgg, TId, TState, TEvent>());
        }

        public static Transition<TAgg, TId, TState> Transition(Func<TState, IAggregateEvent<TAgg, TId>, TState> f)
        {
            return Transition(f.ToStateResult());
        }

        public static Transition<TAgg, TId, TState> Transition<TEvent>(Func<TState, TEvent, TState> f)
            where TEvent : IAggregateEvent<TAgg, TId>
        {
            return Transition(f.ToStateResult()
                .MatchEvent<TAgg, TId, TState, TEvent>());
        }

        public static Transition<TAgg, TId, TState> Transition(Func<IAggregateEvent<TAgg, TId>, TState> f)
        {
            return Transition(f.IgnoreInput(default(TState)).ToStateResult());
        }

        public static Transition<TAgg, TId, TState> Transition<TEvent>(Func<TEvent, TState> f)
            where TEvent : IAggregateEvent<TAgg, TId>
        {
            return Transition(f
                .IgnoreInput(default(TState))
                .ToStateResult()
                .MatchEvent<TAgg, TId, TState, TEvent>());
        }

        public static Transition<TAgg, TId, TState> Transition<TEvent>(
            Func<TransitionResult<TState>> f)
            where TEvent : IAggregateEvent<TAgg, TId>
        {
            return Transition(f
                .IgnoreInput(default(TEvent))
                .IgnoreInput(default(TState))
                .MatchEvent<TAgg, TId, TState, TEvent>());
        }

        public static Transition<TAgg, TId, TState> Transition<TEvent>(
            Func<TState> f)
            where TEvent : IAggregateEvent<TAgg, TId>
        {
            return Transition(f
                .IgnoreInput(default(TEvent))
                .IgnoreInput(default(TState))
                .ToStateResult()
                .MatchEvent<TAgg, TId, TState, TEvent>());
        }

        public static Transition<TAgg, TId, TState> Ignore<TEvent>()
            where TEvent : IAggregateEvent<TAgg, TId>
        {
            return Transition((TEvent _) => TransitionResult.Ignore<TState>());
        }

        public static AggregateDefinition<TAgg, TId, TState> DefineAggregate(
            Func<TState> initialStateFactory,
            IEnumerable<IHandler<TAgg, TId, TState>> handlers,
            IEnumerable<Transition<TAgg, TId, TState>> transitions)
        {
            return new AggregateDefinition<TAgg, TId, TState>(initialStateFactory, handlers, transitions);
        }

        public static IEnumerable<IHandler<TAgg, TId, TState>> Handlers(params IHandler<TAgg, TId, TState>[] handlers)
        {
            return handlers;
        }

        public static IEnumerable<Transition<TAgg, TId, TState>> Transitions(
            params Transition<TAgg, TId, TState>[] transitions)
        {
            return transitions;
        }

        public static void RegisterAggregate(
            AggregateDefinition<TAgg, TId, TState> definition,
            IEventFlowOptions options,
            ICommandHandlerFactory<TAgg, TId, TState> commandHandlerFactory = null)
        {
            var handlerFactory = commandHandlerFactory ?? 
                                 new FnCommandHandlerFactory<TAgg, TId, TState>();

            var transitions = definition.Transitions.ToArray();
            var stateFactory = definition.InitialStateFactory;

            options.RegisterServices(services =>
            {
                var registry = new HandlerRegistry<TAgg, TId, TState>(services, handlerFactory);

                foreach (var handler in definition.Handlers)
                {
                    handler.Register(registry);
                }

                services.Register<FnAggregateRoot<TAgg, TId, TState>.IStateApplier>(_ =>
                    new StateApplier<TAgg, TId, TState>(transitions, stateFactory()));
            });
        }
    }
}
