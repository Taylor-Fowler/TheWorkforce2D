using UnityEngine;
using System.Collections.Generic;
using TheWorkforce.Items.Furnaces;
using TheWorkforce.Items.Read_Only_Data;

namespace TheWorkforce.Items
{
    public class ItemFactory : MonoBehaviour
    {
        private List<ItemData> _itemData;
        public static ItemFactory Instance;

        public Sprite FurnaceSprite;
        public Sprite StoneSprite;
        public Sprite IronOreSprite;
        public Sprite CopperOreSprite;
        public Sprite CoalSprite;

        private void Start()
        {
            Instance = this;
        }

        public void Initialise()
        {
            _itemData = new List<ItemData>();
            _itemData.Add(Furnace.Initialise(FurnaceSprite));
            _itemData.Add(Stone.Initialise(StoneSprite));
            _itemData.Add(IronOre.Initialise(IronOreSprite));
            _itemData.Add(CopperOre.Initialise(CopperOreSprite));
            _itemData.Add(Coal.Initialise(CoalSprite));
        }

        public Furnace CreateFurnace()
        {
            return new Furnace();
        }

        public ItemData Get(ushort id)
        {
            return _itemData[id];
        }

    }
}
