using TheWorkforce.Game_State;

namespace TheWorkforce.Interfaces
{
    public interface ISlot
    {
        ItemStack ItemStack { get; }
        bool Add(ItemStack item);
        ItemStack Remove();

        bool IsEmpty();
        void SubscribeToDirty(DirtyHandler handler);
        void UnsubscribeToDirty(DirtyHandler handler);
    }
}
