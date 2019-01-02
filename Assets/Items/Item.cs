using UnityEngine;

namespace TheWorkforce.Items
{
    public abstract class Item : IItem
    {
        //public int Id { get; private set; }
        //public string Name { get; private set; }
        //public string Description { get; private set; }
        //public EItemType ItemType { get; private set; }
        //public Sprite Icon { get; private set; }
        //public int MaxStackSize { get; private set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EItemType ItemType { get; set; }
        public Sprite Icon { get; set; }
        public int MaxStackSize { get; set; }

        public void InitialiseItem(int id, string name, string description, int itemType, Sprite icon, int maxStackSize = 1)
        {
            Id = id;
            Name = name;
            Description = description;
            ItemType = (EItemType)itemType;
            Icon = icon;
            MaxStackSize = maxStackSize;
        }

        public abstract ItemObject Load(object[] objectData);        
    }
}
