﻿using TheWorkforce.Interfaces;

namespace TheWorkforce.Inventory
{
    public class InputSlot : SlotStrategy
    {
        public InputSlot(ISlot slot) : base(slot)
        {
        }

        public override ItemStack Remove()
        {
            return null;
        }
    }
}
