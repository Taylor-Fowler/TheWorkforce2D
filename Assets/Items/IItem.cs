using UnityEngine;

namespace TheWorkforce
{
    // Types of Item:
    //  - Equipment
    //  - Consumable
    //  - Resource
    //  - Machinery
    //

    public interface IItem
    {
        int ID { get; }
        string Name { get; }
        string Description { get; }
        EItemType ItemType { get; }
        Sprite Icon { get; }
        int MaxStackSize { get; }

        void InitialiseItem(int id, string name, string description, Sprite icon, int maxStackSize = 1);
        //void Display();
        //void Drop();
    }
}
