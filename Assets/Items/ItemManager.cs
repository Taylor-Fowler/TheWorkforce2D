using UnityEngine;
using System.Collections.Generic;
using TheWorkforce.Game_State;

namespace TheWorkforce.Items
{
    public class ItemManager : IManager
    {
        #region IManager Implementation
        public GameManager GameManager { get; private set; }

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
        }
        #endregion

        #region Public Indexers
        public IItem this[int index]
        {
            get
            {
                IItem tryGet = null;
                if(index < 0 || !_itemsMappedToId.TryGetValue(index, out tryGet))
                {
                    return null;
                }
                return tryGet;
            }
        }
        #endregion

        #region Public Properties
        public readonly List<IGeneratable> GeneratableItems;
        public readonly List<IItem> Items;
        #endregion

        #region Private Members
        private readonly List<RawMaterial> _rawMaterials;
        private readonly List<CraftingComponent> _craftingComponents;
        private readonly Dictionary<int, IItem> _itemsMappedToId;
        #endregion

        #region Constructors
        public ItemManager()
        {
            GeneratableItems = new List<IGeneratable>();

            Items = new List<IItem>();
            _rawMaterials = new List<RawMaterial>();
            _craftingComponents = new List<CraftingComponent>();
            _itemsMappedToId = new Dictionary<int, IItem>();
        }
        #endregion

        public void Add(RawMaterial rawMaterial)
        {
            Add(rawMaterial as IItem);
            GeneratableItems.Add(rawMaterial);
            _rawMaterials.Add(rawMaterial);
        }

        public void Add(CraftingComponent craftingComponent)
        {
            Add(craftingComponent as IItem);
            _craftingComponents.Add(craftingComponent);
        }

        private void Add(IItem item)
        {
            Items.Add(item);
            _itemsMappedToId.Add(item.Id, item);
        }

        public IItem RandomItem()
        {
            if (Items == null || Items.Count == 0)
            {
                return null;
            }
            return Items[Random.Range(0, Items.Count)];
        }
    }
}
