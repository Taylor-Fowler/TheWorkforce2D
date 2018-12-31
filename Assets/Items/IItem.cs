using UnityEngine;

// Types of Item:
//  - Equipment
//  - Consumable
//  - RawMaterial
//  - Machinery
//

namespace TheWorkforce.Items
{
    public interface IItem
    {
        int Id { get; }
        string Name { get; }
        string Description { get; }
        EItemType ItemType { get; }
        Sprite Icon { get; }
        int MaxStackSize { get; }

        void InitialiseItem(int id, string name, string description, Sprite icon, int maxStackSize = 1);
        ItemController SpawnObject(Transform parent);
        //void Display();
        //void Drop();
    }
}
