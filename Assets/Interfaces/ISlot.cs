using TheWorkforce.Inventory;

namespace TheWorkforce.Interfaces
{
    public interface ISlot
    {
        event DirtySlot OnDirty;

        ItemStack ItemStack { get; }
        bool Add(ItemStack item);
        ItemStack Remove();

        bool IsEmpty();
    }
}
