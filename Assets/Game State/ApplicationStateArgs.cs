using System;

namespace TheWorkforce.Game_State
{
    public class ApplicationStateArgs : EventArgs
    {
        private readonly EApplicationState _previous;
        private readonly EApplicationState _current;

        public EApplicationState Previous => _previous;
        public EApplicationState Current => _current;
        
        public ApplicationStateArgs(EApplicationState previous, EApplicationState current)
        {
            _previous = previous;
            _current = current;
        }
    }
}