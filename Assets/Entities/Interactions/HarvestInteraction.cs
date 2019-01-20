using System;
using TheWorkforce.Interfaces;

namespace TheWorkforce.Entities.Interactions
{
    public class HarvestInteraction : Interaction
    {
        private readonly EntityData _targetData;
        private readonly Func<bool> _onHarvest;
        private readonly IInventory _initiatorInventory;

        public HarvestInteraction(EntityInstance target, EntityData targetData, Func<bool> onHarvest, IInventory inventory) : base(target, true)
        {
            _targetData = targetData;
            _onHarvest = onHarvest;
            _initiatorInventory = inventory;
            if(onHarvest())
            {
                _initiatorInventory.Inventory.Add(new ItemStack(_targetData, 1));
            }
        }

        public override void ProcessTick(float time)
        {
        }
    }
}
