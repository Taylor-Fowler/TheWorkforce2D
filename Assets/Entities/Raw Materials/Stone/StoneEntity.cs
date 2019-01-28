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
        // TODO: TicksToHarvest should be moved to StoneData
        public ushort TicksToHarvest;

        public StoneEntity(uint id, int x, int y, Action<uint> onDestroy, StoneData data) : base(id, x, y, onDestroy)
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

        public override void Display(EntityView entityView)
        {
            _data.Display(entityView);
            entityView.SetDescription("Harvest Time: " + TicksToHarvest.ToString());
            entityView.SetImageAmount(Amount);
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
            OnDirty();

            if(Amount == 0)
            {
                Destroy();
            }
            return true;
        }

        public override EntityData GetData()
        {
            return _data;
        }
    }
}
