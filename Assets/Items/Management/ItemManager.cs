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
        public ItemInstance this[int index]
        {
            get
            {
                ItemInstance tryGet = null;
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
        public readonly List<ItemInstance> ItemConfigurationInstances;
        #endregion

        #region Private Members
        private readonly Dictionary<int, ItemInstance> _itemsMappedToId;
        #endregion

        #region Constructors
        public ItemManager()
        {
            GeneratableItems = new List<IGeneratable>();

            ItemConfigurationInstances = new List<ItemInstance>();
            _itemsMappedToId = new Dictionary<int, ItemInstance>();
        }
        #endregion

        private void Add(ItemInstance item)
        {
            ItemConfigurationInstances.Add(item);
            _itemsMappedToId.Add(item.ItemDetails.Id, item);
        }

        public ItemInstance RandomItem()
        {
            if (ItemConfigurationInstances == null || ItemConfigurationInstances.Count == 0)
            {
                return null;
            }
            return ItemConfigurationInstances[Random.Range(0, ItemConfigurationInstances.Count)];
        }
    }
}
