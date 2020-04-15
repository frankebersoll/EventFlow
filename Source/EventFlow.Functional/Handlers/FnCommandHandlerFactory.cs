using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Configuration;
using EventFlow.Core;

namespace EventFlow.Functional.Handlers
{
    public class FnCommandHandlerFactory<TAgg, TId, TState> : ICommandHandlerFactory<TAgg, TId, TState>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        public ICommandHandler<TAgg, TId, IExecutionResult, TCommand>
            CreateHandler<TCommand>(Handler<TAgg, TId, TState, TCommand> handler,
                IResolverContext context)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            return new FnCommandHandler<TAgg, TId, TState, TCommand>(handler);
        }
    }
}
