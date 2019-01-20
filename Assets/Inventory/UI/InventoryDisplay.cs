using UnityEngine;
using TheWorkforce.UI;
using TheWorkforce.Inventory;

namespace TheWorkforce
{
    public class SlotCollectionDisplay : MonoBehaviour, IDisplay
    {
        [SerializeField] protected SlotCollection _slotCollection;
        [SerializeField] protected SlotButton[] _inventoryButtons;

        #region Unity API
        protected virtual void Start()
        {
            _inventoryButtons = GetComponentsInChildren<SlotButton>();
            LinkSlots();

            Debug.Log("[InventoryDisplay] - Start() \n" 
                    + gameObject.name + " - _inventoryButtons.Length - " + _inventoryButtons.Length);
        }
        #endregion

        public virtual void SetInventory(SlotCollection slots)
        {
            _slotCollection = slots;
            LinkSlots();
        }

        private void LinkSlots()
        {
            if(_slotCollection != null && _inventoryButtons != null && _inventoryButtons.Length == _slotCollection.Size)
            {
                for (int i = 0; i < _slotCollection.Size; ++i)
                {
                    _inventoryButtons[i].LinkSlot(_slotCollection.GetSlot(i));
                }
            }
        }

        #region IDisplay Implementation
        public virtual void Display() { }
        public virtual void Hide() { }
        #endregion
    }
}
