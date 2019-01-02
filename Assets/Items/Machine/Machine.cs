using UnityEngine;

namespace TheWorkforce.Items
{
    public class Machine : Item//: IItem
    {
        //#region IItem Members
        //public int Id { get; private set; }
        //public string Name { get; private set; }
        //public string Description { get; private set; }
        //public EItemType ItemType { get; private set; }
        //public Sprite Icon { get; private set; }
        //public int MaxStackSize { get; private set; }
        //#endregion

        //public Machine()
        //{
        //    ItemType = EItemType.Machinery;
        //}

        //public void InitialiseItem(int id, string name, string description, Sprite icon, int maxStackSize = 1)
        //{
        //    Id = id;
        //    Name = name;
        //    Description = description;
        //    Icon = icon;
        //    MaxStackSize = maxStackSize;
        //}

        //public ItemController SpawnObject(Transform parent)
        //{
        //    GameObject spawned = new GameObject(Name);
        //    spawned.AddComponent<SpriteRenderer>().sprite = Icon;

        //    ItemController itemController = spawned.AddComponent<ItemController>();
        //    itemController.SetItem(this);

        //    return itemController;
        //}
        public override ItemObject Load(object[] objectData)
        {
            throw new System.NotImplementedException();
        }
    }
}