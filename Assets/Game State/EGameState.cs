namespace TheWorkforce.Game_State
{
    /// <summary>
    /// Game State defines all of the possible states that the game can be in
    /// </summary>
    public enum EGameState
    {
        /// <summary>
        /// The game is not running, i.e. the application is in the menu
        /// </summary>
        NotLoaded,

        /// <summary>
        /// The game data is being loaded from file or being received via the network
        /// </summary>
        Waking,

        /// <summary>
        /// The game data has been received and game objects have been initialised
        /// 
        /// This state is used when a new client has just finished setting up the game and has
        /// just informed the server that it is ready. This state is kept until the server informs
        /// all clients to proceeed.
        /// </summary>
        Initialised,

        /// <summary>
        /// The game is running and proceeding normally 
        /// </summary>
        Active,

        /// <summary>
        /// The game has been paused, initial usage will be for current clients when a new client connects.
        /// All clients will need to pause whilst the new client receives up to date game data.
        /// </summary>
        Paused,

        /// <summary>
        /// The game is syncing, this will be used when one or more clients are out of sync with the server.
        /// </summary>
        Syncing,

        /// <summary>
        /// The game is disconnecting and game data is being unloaded from the client, each client has to clean up their
        /// own application data
        /// </summary>
        Disconnecting
    }
}