using TheWorkforce.Items;

namespace TheWorkforce
{
    public class Player
    {
        //public readonly int Id;
        #region Public Properties
        public Inventory Inventory { get; private set; }
        public Movement Movement { get; private set; }
        public Toolbelt Toolbelt { get; private set; }
        #endregion

        protected PlayerController _controller { get; private set; }

        public Player(PlayerController playerController, Inventory inventory, Toolbelt toolbelt, Movement movement)
        {
            _controller = playerController;
            Inventory = inventory;
            Toolbelt = toolbelt;
            Movement = movement;
        }

        // Player IS A: Entity
        // Entity HAS: 
        //              Equipment
        //              Movement Speed
        //
        //

        // Player HAS:
        //              Inventory
        //              Health
        //              Equipment
        //              Movement Speed
        //
    }    
}
