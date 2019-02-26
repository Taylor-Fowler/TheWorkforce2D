namespace TheWorkforce.Inventory
{
    using Interfaces;

    // TODO: Change the signature of this...in which scenarios will I need to know what was previously held?
    public delegate void DirtySlot(ISlot slot, ItemStack previous);

    public sealed class Slot : ISlot
    {
        public event DirtySlot OnDirty;
        public ItemStack ItemStack { get; private set; }

        public bool IsEmpty => ItemStack == null || ItemStack.IsEmpty();
        public ushort? SpaceLeft => ItemStack?.SpaceLeft();

        public bool Add(ItemStack itemStack)
        {
            // Cannot add an empty ItemStack
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

            var amountAdded = ItemStack.Add(itemStack.Count);

            if (amountAdded > 0)
            {
                itemStack.Subtract(amountAdded);
                Dirty(previous);
            }

            return previous == null || previous.Count != ItemStack.Count;
        }

        public ItemStack Remove()
        {
            if (ItemStack == null)
            {
                return null;
            }

            var value = new ItemStack(ItemStack);
            ItemStack = null;
            Dirty(value);

            return value;
        }

        public ItemStack Remove(ushort count)
        {
            // Cannot remove an item if there is nothing to remove
            if (ItemStack == null)
            {
                return null;
            }

            ushort amountPriorToRemoval = ItemStack.Count;
            ushort amountActuallyRemoved = ItemStack.Subtract(count);

            // Check that something was actually removed
            if(amountActuallyRemoved > 0)
            {
                ItemStack previous = new ItemStack(ItemStack.Item, amountPriorToRemoval);
                if(ItemStack.Count == 0)
                {
                    ItemStack = null;
                }

                Dirty(previous);
                return new ItemStack(previous.Item, amountActuallyRemoved);
            }
            return null;
        }

        #region Custom Event Invoking
        private void Dirty(ItemStack previous)
        {
            OnDirty?.Invoke(this, previous);
        }
        #endregion
    }
}
