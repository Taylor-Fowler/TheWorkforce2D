using UnityEngine;

namespace TheWorkforce.Items
{
    public class RawMaterial : IItem, IHarvestRequirements, IGeneratable
    {
        #region IItem Members
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public EItemType ItemType { get; private set; }
        public Sprite Icon { get; private set; }
        public int MaxStackSize { get; private set; }
        #endregion

        #region IHarvestRequirements Members
        public EToolType HarvestTool { get; private set; }
        public float HarvestSpeed { get; private set; }
        public int HarvestAmount { get; private set; }
        #endregion

        #region IGeneratable Members
        public float MaximumMoisture { get; private set; }
        public float MinimumMoisture { get; private set; }
        public float MaximumElevation { get; private set; }
        public float MinimumElevation { get; private set; }
        #endregion

        public RawMaterial()
        {
            ItemType = EItemType.RawMaterial;
        }

        public void InitialiseItem(int id, string name, string description, Sprite icon, int maxStackSize = 1)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
            MaxStackSize = maxStackSize;
        }

        public void InitialiseHarvestRequirements(EToolType harvestTool, float harvestSpeed, int harvestAmount)
        {
            HarvestTool = harvestTool;
            HarvestSpeed = harvestSpeed;
            HarvestAmount = harvestAmount;
        }

        public void InitialiseGeneratable(float maxMoisture, float minMoisture, float maxElevation, float minElevation)
        {
            MaximumMoisture = maxMoisture;
            MinimumMoisture = minMoisture;
            MaximumElevation = maxElevation;
            MinimumElevation = minElevation;
        }

        public bool CanGenerate(float moisture, float elevation)
        {
            return moisture >= MinimumMoisture 
                && moisture <= MaximumMoisture 
                && elevation >= MinimumElevation
                && elevation <= MaximumElevation;
        }

        public ItemController SpawnObject(Transform parent)
        {
            GameObject spawned = new GameObject(Name);
            spawned.transform.SetParent(parent);
            spawned.transform.position = parent.position;

            spawned.transform.Translate(Vector3.forward * Static_Classes.AssetProvider.TILE_ITEM_Z_INDEX);
            spawned.AddComponent<SpriteRenderer>().sprite = Icon;
            spawned.AddComponent<BoxCollider2D>();

            ItemController itemController = spawned.AddComponent<ItemController>();
            itemController.SetItem(this);
            return itemController;
        }

    }
}
