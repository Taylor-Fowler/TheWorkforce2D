using UnityEngine;

namespace TheWorkforce
{
    public class PlayerInventoryDisplay : SlotCollectionDisplay
    {
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _quickBarPanel;

        public override void Display()
        {
            _inventoryPanel.SetActive(true);
        }

        public override void Hide()
        {
            _inventoryPanel.SetActive(false);
        }
    }
}