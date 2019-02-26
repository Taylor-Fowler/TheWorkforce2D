using TheWorkforce.Inventory;

namespace TheWorkforce.Interfaces
{
    public interface ISlot
    {
        event DirtySlot OnDirty;

        ItemStack ItemStack { get; }
        ushort? SpaceLeft { get; }

        bool Add(ItemStack item);
        ItemStack Remove();
        ItemStack Remove(ushort count);

        bool IsEmpty { get; }
    }
}
