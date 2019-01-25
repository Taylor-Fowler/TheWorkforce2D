using System;
using TheWorkforce.Entities.Interactions;
using TheWorkforce.Interfaces;
using UnityEngine;

namespace TheWorkforce.Entities
{
    public class StoneEntity : EntityInstance, IInteract
    {
        private readonly StoneData _data;

        public ushort Amount;
        public ushort TicksToHarvest;

        public StoneEntity(uint id, Action<uint> onDestroy, StoneData data) : base(id, onDestroy)
        {
            Amount = 10;
            TicksToHarvest = 120;
            _data = data;
        }

        public override uint GetDataTypeId()
        {
            return _data.Id;
        }

        public override GameObject Spawn()
        {
            return _data.Template();
        }

        public override void Display()
        {
            _data.Display();
            _data.ViewLink.View.SetDescription(Amount.ToString());
        }

        public override void Hide()
        {
            _data.Hide();
        }

        public Interaction Interact(EntityInstance initiator)
        {
            IInventory inventory = initiator as IInventory;
            if(inventory != null)
            {
                return new HarvestInteraction(this, _data, DecreaseAmount, inventory, TicksToHarvest);
            }
            return null;
        }

        private bool DecreaseAmount()
        {
            if(Amount == 0)
            {
                return false;
            }
            --Amount;

            if(Amount == 0)
            {
                // kill the entity
                Destroy();
            }
            return true;
        }
    }
}
