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

        public bool Add(ItemStack itemStack)
        {
            if(itemStack == null)
            {
                return false;
            }

            if (ItemStack == null)
            {
                ItemStack = new ItemStack(itemStack.Item, 0);
            }

            bool changed = ItemStack.Add(itemStack);
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
