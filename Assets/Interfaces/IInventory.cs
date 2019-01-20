using TheWorkforce.Inventory;

namespace TheWorkforce.Interfaces
{
    public interface IInventory
    {
        SlotCollection Inventory { get; }
    }
}
