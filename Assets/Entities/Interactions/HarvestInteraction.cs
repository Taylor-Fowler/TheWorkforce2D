using System;
using TheWorkforce.Interfaces;

namespace TheWorkforce.Entities.Interactions
{
    public class HarvestInteraction : Interaction
    {
        private readonly EntityData _targetData;
        private readonly Func<bool> _onHarvest;
        private readonly IInventory _initiatorInventory;
        private readonly ushort _ticksPerHarvest;
        private ushort _ticksUntilNextHarvest;

        public HarvestInteraction(EntityInstance target, EntityData targetData, Func<bool> onHarvest, IInventory inventory, ushort harvestTickTime) : base(target, true)
        {
            _targetData = targetData;
            _onHarvest = onHarvest;
            _initiatorInventory = inventory;
            _ticksPerHarvest = _ticksUntilNextHarvest = harvestTickTime;
        }

        public override void Execute()
        {
            --_ticksUntilNextHarvest;
            if (_ticksUntilNextHarvest == 0)
            {
                _ticksUntilNextHarvest = _ticksPerHarvest;
                if (_onHarvest())
                {
                    _initiatorInventory.Inventory.Add(new ItemStack(_targetData, 1));
                }
            }            
        }

        public override void Display(EntityInteractionDisplay entityInteractionDisplay)
        {
            entityInteractionDisplay.DisplayHarvest(_ticksUntilNextHarvest, _ticksPerHarvest);
        }

        public override void Hide(EntityInteractionDisplay entityInteractionDisplay)
        {
            entityInteractionDisplay.HideHarvest();
        }
    }
}
