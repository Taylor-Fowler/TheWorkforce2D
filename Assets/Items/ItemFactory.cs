using System.Collections.Generic;
using TheWorkforce.Crafting;
using TheWorkforce.Items.Furnaces;
using TheWorkforce.Items.Read_Only_Data;

namespace TheWorkforce.Items
{
    public static class ItemFactory
    {
        private static List<ItemData> _itemData;

        public static void Initialise()
        {
            _itemData = new List<ItemData>();

            Furnace.ItemData = new ItemData("Furnace", "Furnace", null);
            Stone.ItemData = new ItemData("Stone", "Stone", null);
            IronOre.ItemData = new ItemData("Iron Ore", "Iron Ore", null);
            CopperOre.ItemData = new ItemData("Copper Ore", "Copper Ore", null);
            Coal.ItemData = new FuelData("Coal", "Coal", null, new Fuel(5.0f, 5.0f));
        }

        public static Furnace CreateFurnace()
        {
            return new Furnace();
        }

        public static ItemData Get(ushort id)
        {
            return _itemData[id];
        }

    }
}
