namespace TheWorkforce.Interfaces
{
    using Game_State;

    interface IManager
    {
        GameManager GameManager { get; }

        void Startup(GameManager gameManager);
    }
}
