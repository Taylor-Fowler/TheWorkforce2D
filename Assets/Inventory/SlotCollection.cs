using System.Collections.Generic;
using TheWorkforce.Game_State;
using TheWorkforce.Interfaces;

namespace TheWorkforce.Inventory
{
    public class SlotCollection
    {
        #region Custom Event Declaratations
        public event DirtyHandler OnDirty;
        #endregion
        /// <summary>
        /// The size of the inventory
        /// </summary>
        public uint Size { get; protected set; }

        private readonly List<Slot> _slots;
        protected Dictionary<uint, List<uint>> _itemIdFoundInSlots = new Dictionary<uint, List<uint>>();
        
        public SlotCollection(uint size)
        {
            Size = size;
            _slots = new List<Slot>((int)Size);

            for(uint i = 0; i < Size; ++i)
            {
                _slots.Add(new Slot());
            }
        }

        public ISlot GetSlot(int index)
        {
            if(index >= Size)
            {
                return null;
            }
            return _slots[index];
        }

        public virtual void Add(ItemStack itemStack)
        {
            UnityEngine.Debug.Log("[SlotCollection] - Add(ItemStack) \n"
                    + "itemStack: " + itemStack);
            List<uint> slots = GetSlotsWithItemId(itemStack.Item.Id);

            // There are slots with the item id in, therefore we should try to add the 
            // item to that slot
            if(slots != null)
            {
                AddItemToSlotsWithItem(itemStack, slots);
            }
            else
            {
                slots = AddItemIdToSlotsMap(itemStack.Item.Id);
            }

            if(!itemStack.IsNull())
            {
                AddItemToEmptySlots(itemStack, slots);
            }
        }

        public virtual ItemStack Remove(uint slotIndex)
        {
            ItemStack toRemove = null;
            if(slotIndex < Size)
            {
                toRemove = _slots[(int)slotIndex].Remove();
            }

            return toRemove;
        }

        public virtual int NextEmpty()
        {
            for(int i = 0; i < Size; ++i)
            {
                if(_slots[i].IsEmpty())
                {
                    return i;
                }
            }

            return -1;
        }

        public virtual int NextEmpty(int previous)
        {
            for(; previous < Size; ++previous)
            {
                if (_slots[previous].IsEmpty())
                {
                    return previous;
                }
            }

            return -1;
        }

        private void AddItemToSlotsWithItem(ItemStack itemStack, List<uint> slotsWithItem)
        {
            foreach(var slotIndex in slotsWithItem)
            {
                _slots[(int)slotIndex].Add(itemStack);
                if(itemStack.IsNull())
                {
                    return;
                }
            }
        }

        private void AddItemToEmptySlots(ItemStack itemStack, List<uint> registerSlotIds)
        {
            int nextEmpty = NextEmpty();
            while(nextEmpty != -1 && !itemStack.IsNull())
            {
                registerSlotIds.Add((uint)nextEmpty);
                _slots[nextEmpty].Add(itemStack);
                nextEmpty = NextEmpty(nextEmpty);
            }
        }

        private List<uint> GetSlotsWithItemId(uint itemId)
        {
            List<uint> slots = null;
            _itemIdFoundInSlots.TryGetValue(itemId, out slots);
            return slots;
        }

        private List<uint> AddItemIdToSlotsMap(uint itemId)
        {
            List<uint> slots = new List<uint>();
            _itemIdFoundInSlots.Add(itemId, slots);
            return slots;
        }

        #region Custom Event Invoking
        protected virtual void Dirty()
        {
            OnDirty?.Invoke(this);
        }
        #endregion
    }
}