using TheWorkforce.Inventory;
using UnityEngine;

namespace TheWorkforce
{
    public class PlayerInventoryDisplay : SlotCollectionDisplay
    {
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _quickBarPanel;
        [SerializeField] private GameObject _craftingPanel;

        public override void SetInventory(SlotCollection slots)
        {
            base.SetInventory(slots);
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