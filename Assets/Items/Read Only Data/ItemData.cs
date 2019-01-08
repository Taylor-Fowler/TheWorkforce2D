using System.Collections.Generic;
using TheWorkforce.Crafting;
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

        public byte MaxStackSize;

        public ItemData(string name, string description, Sprite sprite, byte maxStackSize = 1)
        {
            Id = ++CurrentId;
            Name = name;
            Description = description;
            Sprite = sprite;
            MaxStackSize = maxStackSize;
        }
    }
}
