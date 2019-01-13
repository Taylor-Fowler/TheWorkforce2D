using TheWorkforce.Interfaces;

namespace TheWorkforce.Inventory
{
    public class OutputSlot : SlotStrategy
    {
        public OutputSlot(ISlot slot) : base(slot)
        {
        }

        public override bool Add(ItemStack item)
        {
            return false;
        }
    }
}
