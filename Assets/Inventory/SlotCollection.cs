using System.Collections.Generic;
using TheWorkforce.Game_State;

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
        

        protected Dictionary<int, List<int>> _itemIdFoundInSlots = new Dictionary<int, List<int>>();


        public SlotCollection(uint size)
        {
            Size = size;
        }

        public virtual void Add()
        {

        }

        public virtual void Remove()
        {

        }

        public virtual int NextEmpty()
        {

            return -1;
        }

        #region Custom Event Invoking
        protected virtual void Dirty()
        {
            OnDirty?.Invoke(this);
        }
        #endregion

    }
}