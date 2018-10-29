using UnityEngine;

namespace TheWorkforce
{
    public class Resource : IItem, IHarvestRequirements
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public EItemType ItemType { get; private set; }
        public Sprite Icon { get; private set; }
        public int MaxStackSize { get; private set; }

        public EToolType HarvestTool { get; private set; }
        public float HarvestSpeed { get; private set; }
        public int HarvestAmount { get; private set; }

        public Resource()
        {
            ItemType = EItemType.Resource;
        }

        public void InitialiseItem(int id, string name, string description, Sprite icon, int maxStackSize = 1)
        {
            ID = id;
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
    }
}
