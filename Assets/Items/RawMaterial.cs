using UnityEngine;

namespace TheWorkforce.Items
{
    public class RawMaterial : IItem, IHarvestRequirements
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
    }
}
