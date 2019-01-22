using System;
using TheWorkforce.Interfaces;

namespace TheWorkforce.Inventory
{
    public delegate void DirtySlot(ISlot slot, ItemStack previous);

    public sealed class Slot : ISlot
    {
        public event DirtySlot OnDirty;

        public ItemStack ItemStack { get; private set; }

        public bool Add(ItemStack itemStack)
        {
            if(itemStack == null)
            {
                return false;
            }

            ItemStack previous = null;

            if (ItemStack == null)
            {
                ItemStack = new ItemStack(itemStack.Item, 0);
            }
            else
            {
                previous = new ItemStack(ItemStack);
            }

            bool changed = ItemStack.Add(itemStack);
            if (changed)
            {
                Dirty(previous);
            }

            return changed;
        }

        public ItemStack Remove()
        {
            if (ItemStack == null)
            {
                return null;
            }
            var previous = new ItemStack(ItemStack);

            ItemStack value = new ItemStack(ItemStack);
            ItemStack.Reset();
            Dirty(previous);

            return value;
        }

        public bool IsEmpty()
        {
            return ItemStack == null || ItemStack.IsNull();
        }

        #region Custom Event Invoking
        private void Dirty(ItemStack previous)
        {
            OnDirty?.Invoke(this, previous);
        }
        #endregion
    }
}
