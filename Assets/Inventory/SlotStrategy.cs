namespace TheWorkforce.Inventory
{
    using Interfaces;

    public abstract class SlotStrategy : ISlot
    {
        public event DirtySlot OnDirty
        {
            add { _slot.OnDirty += value; }
            remove { _slot.OnDirty -= value; }
        }

        public ItemStack ItemStack => _slot.ItemStack;
        public ushort? SpaceLeft => _slot.SpaceLeft;

        protected ISlot _slot;

        public SlotStrategy(ISlot slot)
        {
            _slot = slot;
        }

        public virtual bool Add(ItemStack item) => _slot.Add(item);
        public virtual ItemStack Remove() => _slot.Remove();
        public virtual ItemStack Remove(ushort count) => _slot.Remove(count);
        public virtual bool IsEmpty => _slot.IsEmpty;
    }
}
