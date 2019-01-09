using TheWorkforce.Items.Read_Only_Data;
using TheWorkforce.World;
using UnityEngine;

namespace TheWorkforce.Items
{
    public class Stone
    {
        public static ItemData ItemData { get; private set; }

        public static ItemData Initialise(Sprite sprite)
        {
            ItemData = new ItemData("Stone", "Stone", sprite);
            Generation.Register(new Generatable(0.7f, 0.3f, 0.9f, 0.5f, ItemData.Id));
            return ItemData;
        }

        public ushort Amount { get; private set; }
        public float Strength { get; private set; }

        public Stone() : this(50, 10.0f)
        {
        }

        public Stone(ushort amount, float strength)
        {
            Amount = amount;
            Strength = strength;
        }
    }

    public class IronOre
    {
        public static ItemData ItemData { get; private set; }

        public static ItemData Initialise(Sprite sprite)
        {
            ItemData = new ItemData("Iron Ore", "Iron Ore", sprite);
            Generation.Register(new Generatable(0.5f, 0.1f, 0.7f, 0.4f, ItemData.Id));
            return ItemData;
        }

        public ushort Amount { get; private set; }
        public float Strength { get; private set; }

        public IronOre() : this(50, 10.0f)
        {
        }

        public IronOre(ushort amount, float strength)
        {
            Amount = amount;
            Strength = strength;
        }
    }

    public class CopperOre
    {
        public static ItemData ItemData { get; private set; }

        public static ItemData Initialise(Sprite sprite)
        {
            ItemData = new ItemData("Copper Ore", "Copper Ore", sprite);
            Generation.Register(new Generatable(0.5f, 0.3f, 0.6f, 0.3f, ItemData.Id));
            return ItemData;
        }

        public ushort Amount { get; private set; }
        public float Strength { get; private set; }

        public CopperOre() : this(50, 10.0f)
        {
        }

        public CopperOre(ushort amount, float strength)
        {
            Amount = amount;
            Strength = strength;
        }
    }

    public class Coal
    {
        public static FuelData ItemData { get; private set; }

        public static FuelData Initialise(Sprite sprite)
        {
            ItemData = new FuelData("Coal", "Coal", sprite, new Fuel(5.0f, 5.0f));
            Generation.Register(new Generatable(0.3f, 0.0f, 0.3f, 0.0f, ItemData.Id));
            return ItemData;
        }

        public ushort Amount { get; private set; }
        public float Strength { get; private set; }

        public Coal() : this(50, 10.0f)
        {
        }

        public Coal(ushort amount, float strength)
        {
            Amount = amount;
            Strength = strength;
        }
    }

    public class IronIngot
    {
        public static ItemData ItemData { get; private set; }

        public static ItemData Initialise(Sprite sprite)
        {
            ItemData = new ItemData("Iron Ingot", "Iron Ingot", sprite);
            return ItemData;
        }
    }
}
