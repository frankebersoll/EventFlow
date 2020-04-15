using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Configuration;
using EventFlow.Core;

namespace EventFlow.Functional.Handlers
{
    public class HandlerRegistry<TAgg, TId, TState> : IHandlerRegistry<TAgg, TId, TState>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        private readonly ICommandHandlerFactory<TAgg, TId, TState> _commandHandlerFactory;
        private readonly IServiceRegistration _services;

        public HandlerRegistry(IServiceRegistration services,
            ICommandHandlerFactory<TAgg, TId, TState> commandHandlerFactory)
        {
            _services = services;
            _commandHandlerFactory = commandHandlerFactory;
        }

        public void Register<TCommand>(Handler<TAgg, TId, TState, TCommand> handler)
            where TCommand : ICommand<TAgg, TId, IExecutionResult>
        {
            _services.Register(c => _commandHandlerFactory.CreateHandler(handler, c));
        }
    }
}
