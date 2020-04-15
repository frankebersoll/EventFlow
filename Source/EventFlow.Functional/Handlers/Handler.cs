using System;
using System.Collections.Generic;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;

namespace EventFlow.Functional.Handlers
{
    public struct Handler<TAgg, TId, TState, TCommand> : IHandler<TAgg, TId, TState>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
        where TCommand : ICommand<TAgg, TId, IExecutionResult>
    {
        private readonly Func<TAgg, TState, TCommand, IEnumerable<IAggregateEvent<TAgg, TId>>> _handler;

        public Handler(Func<TAgg, TState, TCommand, IEnumerable<IAggregateEvent<TAgg, TId>>> handler)
        {
            _handler = handler;
        }

        public void Register(IHandlerRegistry<TAgg, TId, TState> registry)
        {
            registry.Register(this);
        }

        public IEnumerable<IAggregateEvent<TAgg, TId>> Invoke(TAgg aggregate, TState state, TCommand command)
        {
            return _handler(aggregate, state, command);
        }
    }
}
