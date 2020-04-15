using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.Functional.Handlers;

namespace EventFlow.Functional
{
    public abstract class FnAggregateRoot<TAgg, TId, TState> : AggregateRoot<TAgg, TId>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        private readonly IStateApplier _stateApplier;

        protected FnAggregateRoot(TId id, IStateApplier stateApplier)
            : base(id)
        {
            _stateApplier = stateApplier;
            Register(stateApplier);
        }

        public void Process<TCommand>(Handler<TAgg, TId, TState, TCommand> handler, TCommand command)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            TState state = _stateApplier.State;
            var events = handler.Invoke(this as TAgg, state, command);
            foreach (var aggregateEvent in events)
            {
                Emit(aggregateEvent);
            }
        }

        public interface IStateApplier : IEventApplier<TAgg, TId>
        {
            TState State { get; }
        }
    }
}
