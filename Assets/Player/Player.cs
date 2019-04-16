namespace TheWorkforce
{
    using Interfaces; using Inventory;

    public class Player : IInventory
    {
        public readonly int Id;
        public SlotCollection Inventory { get; private set; }
        public Movement Movement { get; private set; }

        private PlayerController _playerController;

        public Player(PlayerController playerController, SlotCollection inventory, Movement movement)
        {
            _playerController = playerController;
            Inventory = inventory;
            Movement = movement;
        }

        public void SetController(PlayerController playerController)
        {
            _playerController = playerController;
        }
    }
}
