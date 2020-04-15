using System.Collections.Generic;
using EventFlow.Aggregates;
using EventFlow.Core;

namespace EventFlow.Functional.Transitions
{
    public class StateApplier<TAgg, TId, TState>
        : FnAggregateRoot<TAgg, TId, TState>.IStateApplier
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        private readonly Transition<TAgg, TId, TState>[] _transitions;

        public StateApplier(Transition<TAgg, TId, TState>[] transitions,
            TState initialState)
        {
            _transitions = transitions;
            State = initialState;
        }

        public bool Apply(TAgg aggregate, IAggregateEvent<TAgg, TId> aggregateEvent)
        {
            var result = Apply(aggregateEvent, _transitions, State);
            switch (result.Type)
            {
                case StateResultType.Transition:
                    State = result.NewState;
                    break;
                case StateResultType.Ignore:
                    break;
                case StateResultType.NoResult:
                    return false;
            }

            return true;
        }

        public TState State { get; private set; }

        private static TransitionResult<TState> Apply(
            IAggregateEvent<TAgg, TId> aggregateEvent,
            IEnumerable<Transition<TAgg, TId, TState>> transitions,
            TState state)
        {
            foreach (var applier in transitions)
            {
                var result = applier(state, aggregateEvent);
                switch (result.Type)
                {
                    case StateResultType.Transition:
                    case StateResultType.Ignore:
                        return result;
                }
            }

            return TransitionResult.NoResult<TState>();
        }
    }
}
