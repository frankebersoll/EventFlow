using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Configuration;
using EventFlow.Core;

namespace EventFlow.Functional.Handlers
{
    public interface ICommandHandlerFactory<TAgg, TId, TState>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        ICommandHandler<TAgg, TId, IExecutionResult, TCommand> CreateHandler<TCommand>(
            Handler<TAgg, TId, TState, TCommand> handler, IResolverContext context)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>;
    }
}
