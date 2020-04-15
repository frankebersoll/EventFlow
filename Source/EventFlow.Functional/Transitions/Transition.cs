using EventFlow.Aggregates;
using EventFlow.Core;

namespace EventFlow.Functional.Transitions
{
    public delegate TransitionResult<TState> Transition<TAgg, TId, TState>(TState state, IAggregateEvent<TAgg, TId> e)
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity;
}
