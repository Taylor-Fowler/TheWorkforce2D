using UnityEngine;

namespace TheWorkforce
{
    public class PlayerInventoryDisplay : SlotCollectionDisplay
    {
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _quickBarPanel;
        [SerializeField] private GameObject _craftingPanel;

        protected override void Start()
        {
            base.Start();
        }

        public override void Display()
        {
            _inventoryPanel.SetActive(true);
            _craftingPanel.SetActive(true);
        }

        public override void Hide()
        {
            _inventoryPanel.SetActive(false);
            _craftingPanel.SetActive(false);
        }
    }
}