using UnityEngine;
using TheWorkforce.UI;
using TheWorkforce.Inventory;

namespace TheWorkforce
{
    public class SlotCollectionDisplay : MonoBehaviour, IDisplay
    {
        protected SlotCollection _slots;
        protected SlotButton[] _inventoryButtons;

        #region Unity API
        private void Start()
        {
            _inventoryButtons = GetComponentsInChildren<SlotButton>();

            Debug.Log("[InventoryDisplay] - Start() \n" 
                    + gameObject.name + " - _inventoryButtons.Length - " + _inventoryButtons.Length);
        }
        #endregion

        public virtual void SetInventory(SlotCollection slots)
        {
            _slots = slots;
        }

        #region IDisplay Implementation
        public virtual void Display() { }
        public virtual void Hide() { }
        #endregion
    }
}
