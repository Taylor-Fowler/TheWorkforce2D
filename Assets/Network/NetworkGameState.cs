using UnityEngine.Networking;

namespace TheWorkforce.Network
{
    using Game_State;

    public class NetworkGameState : MessageBase
    {
        public EGameState Previous;
        public EGameState Current;

        /// <summary>
        /// The agreed time to change the game state, the game state cannot just change immediately otherwise
        /// clients will change game state earlier than others.
        /// </summary>
        public uint Time;
    }

    public class NetworkStateConfirmation : MessageBase
    {
        public EGameState NewState;
    }
}
