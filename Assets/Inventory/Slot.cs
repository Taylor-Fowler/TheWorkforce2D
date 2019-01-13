using TheWorkforce.Game_State;
using TheWorkforce.Interfaces;

namespace TheWorkforce.Inventory
{
    public sealed class Slot : ISlot
    {
        #region Custom Event Declaration
        private event DirtyHandler OnDirty;
        #endregion

        public ItemStack ItemStack { get; private set; }

        public bool Add(ItemStack item)
        {
            if (ItemStack == null)
            {
                ItemStack = new ItemStack(item.Item, 0);
            }

            if (ItemStack.IsNull())
            {
                ItemStack.Copy(item);
                item.Reset();
                Dirty();
                return true;
            }

            bool changed = ItemStack.Add(item);
            if (changed)
            {
                Dirty();
            }

            return changed;
        }

        public ItemStack Remove()
        {
            if (ItemStack == null)
            {
                return null;
            }

            ItemStack value = new ItemStack(ItemStack);
            ItemStack.Reset();
            Dirty();

            return value;
        }

        public bool IsEmpty()
        {
            return ItemStack == null || ItemStack.IsNull();
        }

        #region Custom Event Invoking
        public void SubscribeToDirty(DirtyHandler handler)
        {
            OnDirty += handler;
        }

        public void UnsubscribeToDirty(DirtyHandler handler)
        {
            OnDirty -= handler;
        }

        private void Dirty()
        {
            OnDirty?.Invoke(this);
        }

        #endregion
    }
}
