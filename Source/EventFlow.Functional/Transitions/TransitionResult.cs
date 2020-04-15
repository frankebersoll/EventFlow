namespace EventFlow.Functional.Transitions
{
    public static class TransitionResult
    {
        public static TransitionResult<T> Transition<T>(T newState)
        {
            return new TransitionResult<T>(newState);
        }

        public static TransitionResult<T> Ignore<T>()
        {
            return new TransitionResult<T>(StateResultType.Ignore);
        }

        public static TransitionResult<T> NoResult<T>()
        {
            return new TransitionResult<T>(StateResultType.NoResult);
        }
    }

    public struct TransitionResult<T>
    {
        public StateResultType Type { get; }
        public T NewState { get; }

        internal TransitionResult(T newState)
        {
            Type = StateResultType.Transition;
            NewState = newState;
        }

        internal TransitionResult(StateResultType type)
        {
            Type = type;
            NewState = default;
        }
    }
}
