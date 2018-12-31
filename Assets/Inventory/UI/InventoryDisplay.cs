using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheWorkforce.UI;

namespace TheWorkforce
{
    public class InventoryDisplay : MonoBehaviour, IDisplay
    {
        protected Inventory _inventory;
        protected InventoryButton[] _inventoryButtons;

        #region Unity API
        private void Start()
        {
            _inventoryButtons = GetComponentsInChildren<InventoryButton>();

            Debug.Log("[InventoryDisplay] - Start() \n" 
                    + gameObject.name + " - _inventoryButtons.Length - " + _inventoryButtons.Length);
        }
        #endregion

        public virtual void SetInventory(Inventory inventory)
        {
            _inventory = inventory;
        }

        #region IDisplay Implementation
        public virtual void Display() { }
        public virtual void Hide() { }
        #endregion
    }
}
