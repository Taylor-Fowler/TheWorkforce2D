using TheWorkforce.Game_State;

namespace TheWorkforce
{
    public class InventorySlot
    {
        #region Custom Event Declaration
        public event DirtyHandler OnDirty;
        #endregion

        public ItemStack ItemStack { get; protected set; }
        
        public virtual bool Add(ItemStack item)
        {
            if(ItemStack == null)
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
            if(ItemStack == null)
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

        public static InventorySlot operator --(InventorySlot value)
        {
            if(value.ItemStack == null)
            {
                return value;
            }

            value.ItemStack--;
            value.Dirty();
            return value;
        }

        #region Custom Event Invoking
        protected virtual void Dirty()
        {
            OnDirty?.Invoke(this);
        }
        #endregion
    }

    //public class InventorySlotRequirement : InventorySlot
    //{
    //    private ItemAttributes _requirements = null;

    //    public InventorySlotRequirement(ItemAttributes requirements)
    //    {
    //        _requirements = requirements;
    //    }

    //    public override bool Add(ItemStack item)
    //    {
    //        if (!item.Item.HasAttributes(_requirements))
    //            return false;

    //        bool changed = base.Add(item);
    //        if (changed)
    //            this.OnDirty();

    //        return changed;
    //    }
    //}
}