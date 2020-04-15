using EventFlow.Core;

namespace EventFlow.Functional.Handlers
{
    public interface IHandler<TAgg, TId, TState>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        void Register(IHandlerRegistry<TAgg, TId, TState> registry);
    }
}
