namespace TheWorkforce.Inventory
{
    public class ConstrainedSlot<T> : SlotStrategy where T : class
    {

        public ConstrainedSlot(ISlot slot) : base(slot)
        {
        }
        
        public T GetItem()
        {
            return _slot.ItemStack.Item as T;
        }
        public override bool Add(ItemStack item)
        {
            if(item.Item is T)
            {
                return base.Add(item);
            }

            return false;
        }
    }
}
