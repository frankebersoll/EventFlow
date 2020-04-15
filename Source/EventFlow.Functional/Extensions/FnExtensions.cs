using System;
using System.Collections.Generic;
using EventFlow.Aggregates;
using EventFlow.Core;
using EventFlow.Functional.Transitions;

namespace EventFlow.Functional.Extensions
{
    public static class FnExtensions
    {
        public static Func<T, TResult> IgnoreInput<T, TResult>(
            // ReSharper disable once UnusedParameter.Global
            this Func<TResult> f, T _)
        {
            return e => f();
        }

        public static Func<T, T1, TResult> IgnoreInput<T, T1, TResult>(
            // ReSharper disable once UnusedParameter.Global
            this Func<T1, TResult> f, T _)
        {
            return (__, t1) => f(t1);
        }

        public static Func<T, T1, T2, TResult> IgnoreInput<T, T1, T2, TResult>(
            // ReSharper disable once UnusedParameter.Global
            this Func<T1, T2, TResult> f, T _)
        {
            return (__, t1, t2) => f(t1, t2);
        }

        public static Func<T1, T2, TransitionResult<TState>> ToStateResult<T1, T2, TState>(
            this Func<T1, T2, TState> f)
        {
            return (t1, t2) =>
            {
                TState result = f(t1, t2);
                return result != null
                    ? TransitionResult.Transition(result)
                    : TransitionResult.NoResult<TState>();
            };
        }

        public static Func<T1, T2, T3, IEnumerable<TResult>> ToEnumerable<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> f)
            where TResult : IAggregateEvent
        {
            IEnumerable<TResult> Func(T1 t1, T2 t2, T3 t3)
            {
                TResult result = f(t1, t2, t3);
                if (result == null) yield break;
                yield return result;
            }

            return Func;
        }

        public static Func<TState, IAggregateEvent<TAgg, TId>, TransitionResult<TState>>
            MatchEvent<TAgg, TId, TState, TEvent>(this Func<TState, TEvent, TransitionResult<TState>> f)
            where TAgg : FnAggregateRoot<TAgg, TId, TState>
            where TId : IIdentity
            where TEvent : IAggregateEvent<TAgg, TId>
        {
            return (state, aggregateEvent) =>
            {
                if (aggregateEvent is TEvent e) return f(state, e);

                return TransitionResult.NoResult<TState>();
            };
        }
    }
}
