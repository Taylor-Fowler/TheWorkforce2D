using TheWorkforce.Game_State;
using TheWorkforce.Interfaces;

namespace TheWorkforce.Inventory
{
    public abstract class SlotStrategy : ISlot
    {
        protected ISlot _slot;

        public event DirtySlot OnDirty
        {
            add { _slot.OnDirty += value; }
            remove { _slot.OnDirty -= value; }
        }

        public SlotStrategy(ISlot slot)
        {
            _slot = slot;
        }

        public ItemStack ItemStack
        {
            get
            {
                return _slot.ItemStack;
            }
        }

        public virtual bool Add(ItemStack item)
        {
            return _slot.Add(item);
        }

        public virtual ItemStack Remove()
        {
            return _slot.Remove();
        }

        public virtual bool IsEmpty()
        {
            return _slot.IsEmpty();
        }
    }
}
