using TheWorkforce.Items.Read_Only_Data;
using UnityEngine;

namespace TheWorkforce.Items
{
    // TODO: Instead of spawning the prefab and then adding this class, have the item classes spawn the prefab and return the new game object,
    //       they will need a reference to the tile controller/s needed
    public class ItemController : MonoBehaviour
    {
        public ItemData Item { get; private set; }

        public virtual void SetItem(ItemData item)
        {
            Item = item;
        }

        public virtual void DisplayItem(ItemInspector itemInspector)
        {
            itemInspector.Display();
            itemInspector.ItemImage.sprite = Item.Sprite;
            itemInspector.ItemName.text = Item.Name;
            itemInspector.ItemDescription.text = Item.Description;
        }
    }
}
