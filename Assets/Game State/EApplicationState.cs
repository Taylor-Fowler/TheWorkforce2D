namespace TheWorkforce.Game_State
{
    /// <summary>
    /// EApplication State defines all of the possible states that the Application can be in.
    /// </summary>
    public enum EApplicationState
    {
        /// <summary>
        /// The initial state of the application, Launching means that the application has just started executing.
        /// Assets are initialised whilst in this state.
        /// </summary>
        Launching,

        /// <summary>
        /// The Menu state is a safe state which defines that any game data has been freed from memory and that
        /// creating/joining games is once again permitted.
        /// 
        /// Valid Transitions:
        /// Launching -> Menu
        /// ReturningToMenu -> Menu
        /// </summary>
        Menu,

        /// <summary>
        /// 
        /// </summary>
        Connecting,

        /// <summary>
        /// 
        /// </summary>
        Loading,

        /// <summary>
        /// 
        /// </summary>
        Ingame,

        /// <summary>
        /// 
        /// </summary>
        ReturningToMenu
    };
}