using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    public class PlayerInventoryDisplay : InventoryDisplay
    {
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _quickBarPanel;
        [SerializeField] private GameObject _craftingPanel;

        public override void SetInventory(Inventory inventory)
        {
            base.SetInventory(inventory);
            _inventoryPanel.SetActive(false);
            _craftingPanel.SetActive(false);
        }

        public override void Display()
        {
            base.Display();
            _inventoryPanel.SetActive(true);
            _craftingPanel.SetActive(true);
        }

        public override void Hide()
        {
            base.Hide();
            _inventoryPanel.SetActive(false);
            _craftingPanel.SetActive(false);
        }
    }
}