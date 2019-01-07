using UnityEngine;
using TheWorkforce.World;

namespace TheWorkforce.Items
{
    //public interface IItem
    //{
    //    ItemDetails ItemDetails { get; }

    //    void Spawn(Tile tile);

    //}

    public abstract class ItemInstance
    {
        public readonly ItemDetails ItemDetails;

        protected Tile _onTile;

        public ItemInstance(ItemDetails details)
        {
            ItemDetails = details;
        }

        public abstract void Spawn(Tile tile);
    }

    public sealed class ItemDetails
    {
        public readonly int Id;
        public readonly string Name;
        public readonly string Description;
        public readonly Sprite Icon;
        public readonly int MaxStackSize;
        public readonly byte Width;
        public readonly byte Height;

        public ItemDetails(int id, string name, string description, Sprite icon, int maxStackSize, byte width, byte height)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
            MaxStackSize = maxStackSize;
            Width = width;
            Height = height;
        }
    }
}
