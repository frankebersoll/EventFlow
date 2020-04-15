using System;
using System.Collections.Generic;
using EventFlow.Core;
using EventFlow.Functional.Handlers;
using EventFlow.Functional.Transitions;

namespace EventFlow.Functional
{
    public class AggregateDefinition<TAgg, TId, TState>
        where TAgg : FnAggregateRoot<TAgg, TId, TState>
        where TId : IIdentity
    {
        public AggregateDefinition(
            Func<TState> initialStateFactory,
            IEnumerable<IHandler<TAgg, TId, TState>> registrations,
            IEnumerable<Transition<TAgg, TId, TState>> transitions)
        {
            InitialStateFactory = initialStateFactory ?? throw new ArgumentNullException(nameof(initialStateFactory));
            Transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
            Handlers = registrations ?? throw new ArgumentNullException(nameof(registrations));
        }

        public Func<TState> InitialStateFactory { get; }
        public IEnumerable<IHandler<TAgg, TId, TState>> Handlers { get; }
        public IEnumerable<Transition<TAgg, TId, TState>> Transitions { get; }
    }
}
