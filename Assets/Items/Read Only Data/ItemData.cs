using System;
using UnityEngine;

namespace TheWorkforce.Items.Read_Only_Data
{
    public class ItemData
    {
        private static ushort CurrentId = 0;

        public readonly ushort Id;
        public readonly string Name;
        public readonly string Description;
        public readonly Sprite Sprite;

        public Type MonoType;

        public byte MaxStackSize;

        public ItemData(string name, string description, Sprite sprite, byte maxStackSize = 1)
        {
            Id = ++CurrentId;
            Name = name;
            Description = description;
            Sprite = sprite;
            MaxStackSize = maxStackSize;
        }

        public virtual void Display(ItemDataView dataView)
        {
            dataView.Display(this);
        }

        public Action DoSomething;

        


        // grid of random items
        // spawn item puts it on map, looks up the monobehaviour to add and adds it
        // each item has a monobehaviour that simply displays the correct view
        // the view just waits for a reference to the object to display and displays it...providing the interface to interact with the object

        // an item in a slot needs to be viewed also, so that the item can be inspected further. 
        // the item, however, is simply a 
    }
}
