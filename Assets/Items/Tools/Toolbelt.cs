using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Items
{
    public class Toolbelt
    {
        #region Public Properties
        public int Size { get { return _toolSlots.Count; } }
        #endregion

        #region Indexers
        public ToolSlot this[int index]
        {
            get
            {
                if(index >= Size || index < 0)
                {
                    return null;
                }

                return _toolSlots[index];
            }
        }
        #endregion

        #region Private Members
        private readonly List<ToolSlot> _toolSlots;
        #endregion

        #region Constructors
        public Toolbelt(IEnumerable<EToolType> slotTypes)
        {
            _toolSlots = new List<ToolSlot>();

            foreach(var toolType in slotTypes)
            {
                _toolSlots.Add(new ToolSlot(toolType));
            }
        }
        #endregion
    } 
}
