using TheWorkforce.Entities;
using TheWorkforce.Interfaces;
using TheWorkforce.Inventory;
using UnityEngine;

namespace TheWorkforce
{
    public class Player : EntityInstance, IInventory
    {
        //public readonly int Id;
        #region Public Properties
        public SlotCollection Inventory { get; private set; }
        public Movement Movement { get; private set; }
        //public Toolbelt Toolbelt { get; private set; }
        #endregion

        protected PlayerController _controller { get; private set; }

        //public Player(PlayerController playerController, SlotCollection inventory, Toolbelt toolbelt, Movement movement)
        public Player(PlayerController playerController, SlotCollection inventory, Movement movement) : base((uint)playerController.Id, 0, 0, null)
        {
            _controller = playerController;
            Inventory = inventory;
            //Toolbelt = toolbelt;
            Movement = movement;
        }

        public override GameObject Spawn()
        {
            return null;
        }

        public override void Display(EntityView entityView)
        {
        }

        public override void Hide()
        {
        }

        public override uint GetDataTypeId()
        {
            throw new System.NotImplementedException();
        }

        public override EntityData GetData()
        {
            throw new System.NotImplementedException();
        }
    }    
}
