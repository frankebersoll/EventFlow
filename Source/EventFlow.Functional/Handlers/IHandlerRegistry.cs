using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;

namespace EventFlow.Functional.Handlers
{
    public interface IHandlerRegistry<TAgg, TId, TState>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        void Register<TCommand>(Handler<TAgg, TId, TState, TCommand> handler)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>;
    }
}
