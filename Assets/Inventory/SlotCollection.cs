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

        private readonly List<ISlot> _slots;
        protected Dictionary<uint, List<uint>> _itemIdFoundInSlots = new Dictionary<uint, List<uint>>();
        
        // two ways the slot can have items added
        // directly to the inventory which then chooses the slot
        // directly to the slot


        public SlotCollection(uint size)
        {
            Size = size;
            _slots = new List<ISlot>((int)Size);

            for(uint i = 0; i < Size; ++i)
            {
                var slot = new Slot();
                _slots.Add(slot);
                slot.OnDirty += Slot_OnDirty;
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

        /// <summary>
        /// Attempts to add a stack of items to any available slots within the collection
        /// </summary>
        /// <param name="stackToAdd">The stack of items to add</param>
        public virtual void Add(ItemStack stackToAdd)
        {
            var slots = GetSlotsForItem(stackToAdd);
            AddItemToSlotsWithItem(stackToAdd, slots);

            if (!stackToAdd.IsEmpty())
            {
                AddItemToEmptySlots(stackToAdd, slots);
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

        /// <summary>
        /// Find the next empty slot within the collection
        /// </summary>
        /// <param name="previous">Default: 0. The starting index to begin searching from</param>
        /// <returns>The index of the next empty slot</returns>
        public virtual int NextEmpty(int previous = 0)
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

        /// <summary>
        /// Looks for the given item within the _itemIdFoundInSlots collection
        /// </summary>
        /// <param name="itemStack">The item to look for</param>
        /// <returns>A list of uints that represent the indices of slots that contain the item</returns>
        private List<uint> GetSlotsForItem(ItemStack itemStack)
        {
            if(itemStack == null)
            {
                return null;
            }

            List<uint> slots = null;
            _itemIdFoundInSlots.TryGetValue(itemStack.Item.Id, out slots);

            // The item Id has not been registered (i.e. has never been attempted to be added to the collection)
            // So register the item Id and get the newly created list
            if (slots == null)
            {
                slots = AddItemIdToSlotsMap(itemStack.Item.Id);
            }

            return slots;
        }

        private void AddItemToSlotsWithItem(ItemStack itemStack, List<uint> slotsWithItem)
        {
            foreach(var slotIndex in slotsWithItem)
            {
                _slots[(int)slotIndex].Add(itemStack);
                if(itemStack.IsEmpty())
                {
                    return;
                }
            }
        }

        private void AddItemToEmptySlots(ItemStack itemStack, List<uint> registerSlotIds)
        {
            int nextEmpty = NextEmpty();
            while(nextEmpty != -1 && !itemStack.IsEmpty())
            {
                //registerSlotIds.Add((uint)nextEmpty);
                _slots[nextEmpty].Add(itemStack);
                nextEmpty = NextEmpty(++nextEmpty);
            }
        }

        /// <summary>
        /// Adds a new list of uints to the _itemIdFoundInSlots collection with the item Id given as the key
        /// </summary>
        /// <param name="itemId">The key of the new pair in the dictionary</param>
        /// <returns>An empty list that should have slot indices added to it</returns>
        private List<uint> AddItemIdToSlotsMap(uint itemId)
        {
            List<uint> slots = new List<uint>();
            _itemIdFoundInSlots.Add(itemId, slots);
            return slots;
        }

        private void RemoveSlotIndexFromSlotsMap(uint slotIndex, uint itemId)
        {
            List<uint> slots;
            if(_itemIdFoundInSlots.TryGetValue(itemId, out slots))
            {
                slots.Remove(slotIndex);
            }
        }

        #region Custom Event Invoking/Response
        protected virtual void Dirty()
        {
            OnDirty?.Invoke(this);
        }

        private void Slot_OnDirty(ISlot slot, ItemStack previous)
        {
            if (slot.ItemStack == null || previous == null || slot.ItemStack.Item != previous.Item)
            {
                var index = _slots.IndexOf(slot);

                // if there was previously another item in the slot, remove its mapping
                if (previous != null && previous.Item != null)
                {
                    RemoveSlotIndexFromSlotsMap((uint)index, previous.Item.Id);
                }

                // if there is now a valid item in the slot then register the slot index in the mapping
                if(slot.ItemStack != null && slot.ItemStack.Item != null)
                {
                    List<uint> slots = GetSlotsForItem(slot.ItemStack);
                    slots?.Add((uint)index);
                }
            }
        }
        #endregion
    }
}