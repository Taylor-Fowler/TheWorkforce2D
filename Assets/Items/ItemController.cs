using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Items
{
    // TODO: Instead of spawning the prefab and then adding this class, have the item classes spawn the prefab and return the new game object,
    //       they will need a reference to the tile controller/s needed
    public class ItemController : MonoBehaviour
    {
        public IItem Item { get; private set; }

        public virtual void SetItem(IItem item)
        {
            Item = item;
        }

        public virtual void DisplayItem(ItemInspector itemInspector)
        {
            itemInspector.Display();
            itemInspector.ItemImage.sprite = Item.Icon;
            itemInspector.ItemName.text = Item.Name;
            itemInspector.ItemDescription.text = Item.Description;
        }
    }
}
