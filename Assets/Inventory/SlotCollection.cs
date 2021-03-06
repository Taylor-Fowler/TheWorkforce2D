using System;
using System.Collections.Generic;

namespace TheWorkforce.Inventory
{
    using Interfaces; using Entities;

    public class SlotCollection
    {
        public event Action<SlotCollection> OnDirty;

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

        public SlotCollection(uint size, List<Tuple<ushort, ushort>> items)
        {
            Size = size;
            _slots = new List<ISlot>((int)Size);

            for (uint i = 0; i < Size; ++i)
            {
                var slot = new Slot();
                _slots.Add(slot);
                slot.OnDirty += Slot_OnDirty;

                // Add the collection of items to the new slot collection
                ushort itemCount = items[(int)i].Item2;
                if(itemCount > 0)
                {
                    ItemStack item = new ItemStack(EntityCollection.Instance.DataMappedToId[items[(int)i].Item1], itemCount);
                    slot.Add(item);
                }
            }
        }

        public List<Tuple<ushort, ushort>> GetSaveData()
        {
            List<Tuple<ushort, ushort>> saveData = new List<Tuple<ushort, ushort>>((int)Size);
            Tuple<ushort, ushort> emptySlot = new Tuple<ushort, ushort>(0, 0);

            foreach(var slot in _slots)
            {
                // If the slot is empty, add the empty slot data, otherwise add the slot's item data
                saveData.Add
                (
                    slot.IsEmpty ?
                        emptySlot : new Tuple<ushort, ushort>(slot.ItemStack.Item.Id, slot.ItemStack.Count)
                );
            }

            return saveData;
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
                if (_slots[previous].IsEmpty)
                {
                    return previous;
                }
            }
            return -1;
        }

        public bool TransactionalAdd(ItemStack itemStack)
        {
            var slotIndices = GetSlotsForItem(itemStack);
            List<Tuple<int, ushort>> transactions = new List<Tuple<int, ushort>>();

            // Try to add the item to all slots that already contain the item
            foreach(var slotIndex in slotIndices)
            {
                var slot = _slots[(int)slotIndex];
                var isSpaceLeft = slot.SpaceLeft;
                // Make sure there is space left
                if(isSpaceLeft != null)
                {
                    ushort spaceLeft = (ushort)isSpaceLeft;
                    if(spaceLeft > 0)
                    {
                        // We only want to add as much as there is to add, dont just fill up the slot because there is space
                        if(spaceLeft > itemStack.Count)
                        {
                            spaceLeft = itemStack.Count;
                        }
                        slot.Add(itemStack);
                        transactions.Add(new Tuple<int, ushort>((int)slotIndex, spaceLeft));
                    }
                }

                if(itemStack.IsEmpty())
                {
                    return true;
                }
            }

            // Try to add the item to any of the empty slots
            int nextEmpty = NextEmpty();
            while (nextEmpty != -1 && !itemStack.IsEmpty())
            {
                ushort count = itemStack.Count;
                _slots[nextEmpty].Add(itemStack);
                transactions.Add(new Tuple<int, ushort>(nextEmpty, (ushort)(count - itemStack.Count)));

                nextEmpty = NextEmpty(++nextEmpty);
            }

            if (itemStack.IsEmpty())
            {
                return true;
            }

            Rollback(transactions);
            return false;
        }

        public bool TransactionalRemove(EditorItemStack[] editorItemStacks)
        {
            // Each transaction consists of a slot ID to identify where the items came from and an
            // ItemStack which defines what was removed
            List<Tuple<int, ItemStack>> transactions = new List<Tuple<int, ItemStack>>();
            //int[] transactions = new int[editorItemStacks.Length * 3];
            bool success = true;

            // Go through each required item
            foreach(var itemStack in editorItemStacks)
            {
                // Track how many of the item we still need
                ushort count = itemStack.Count;

                // Find all of the slots that contain the item we are looking for
                var slots = GetSlotsForItem(itemStack.Item.Id);

                // If there are slots with the item we need then:
                //      - Loop over each slot
                //          - Remove as many items from the slot as we can (never removing more than we need)
                //          - 
                if(slots != null)
                {
                    foreach (var slotIndex in slots)
                    {
                        ISlot slot = _slots[(int)slotIndex];
                        ushort slotCount = slot.ItemStack.Count;
                        ItemStack removed = null;

                        // If there are more in the slot than we need, remove the exact amount we need
                        if (slotCount > count)
                        {
                            removed = slot.Remove(count);
                            slotCount = count; // reuse the variable to track how many we removed
                        }
                        else
                        {
                            // NOTE: This is where the current error is branching to before executing
                            removed = slot.Remove();
                        }

                        // If there was an item to remove, track the transaction as (slot affected, number of items removed)
                        if (removed != null)
                        {
                            transactions.Add(new Tuple<int, ItemStack>((int)slotIndex, removed));
                        }
                        // Reduce the count by the amount of items we just removed from the current slot
                        count -= slotCount;
                        // If we have removed all of the items we need, then stop looping over the slots with the item we needed
                        if (count == 0)
                        {
                            break;
                        }
                    }
                }
                
                // Now that we have either looped over all the slots for the item we currently need, check if we removed the correct amount
                if(count > 0)
                {
                    success = false;
                    break;
                }
            }

            if(!success)
            {
                Rollback(transactions);
            }

            return success;
        }

        private void Rollback(List<Tuple<int, ItemStack>> transactions)
        {
            foreach(var transaction in transactions)
            {
                _slots[transaction.Item1].Add(transaction.Item2);
            }
        }

        private void Rollback(List<Tuple<int, ushort>> transactions)
        {
            foreach(var transaction in transactions)
            {
                _slots[transaction.Item1].Remove(transaction.Item2);
            }
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

        private List<uint> GetSlotsForItem(ushort id)
        {
            List<uint> slots = null;
            _itemIdFoundInSlots.TryGetValue(id, out slots);
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