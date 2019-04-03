using System;

namespace TheWorkforce.Game_State
{
    public class GameStateChangeArgs : EventArgs
    {
        private readonly EGameState _previous;
        private readonly EGameState _current;

        public EGameState Previous => _previous;
        public EGameState Current => _current;
        
        public GameStateChangeArgs(EGameState previous, EGameState current)
        {
            _previous = previous;
            _current = current;
        }
    }
}