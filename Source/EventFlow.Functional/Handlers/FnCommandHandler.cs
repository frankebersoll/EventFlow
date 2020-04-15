using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;

namespace EventFlow.Functional.Handlers
{
    public class FnCommandHandler<TAgg, TId, TState, TCommand> : CommandHandler<TAgg, TId, TCommand>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
        where TCommand : ICommand<TAgg, TId, IExecutionResult>
    {
        private readonly Handler<TAgg, TId, TState, TCommand> _handler;

        public FnCommandHandler(Handler<TAgg, TId, TState, TCommand> handler)
        {
            _handler = handler;
        }

        public override Task ExecuteAsync(TAgg aggregate, TCommand command, CancellationToken cancellationToken)
        {
            aggregate.Process(_handler, command);
            return Task.CompletedTask;
        }
    }
}
