using System;

namespace TheWorkforce.Game_State
{
    public class ApplicationStateChangeArgs : EventArgs
    {
        public EApplicationState Previous { get; }
        public EApplicationState Current { get; }

        public ApplicationStateChangeArgs(EApplicationState previous, EApplicationState current)
        {
            Previous = previous;
            Current = current;
        }
    }
}