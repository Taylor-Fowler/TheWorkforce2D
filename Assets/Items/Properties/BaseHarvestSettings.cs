namespace TheWorkforce.Items
{
    public class BaseHarvestSettings : IHarvestSettings
    {
        public EToolType ToolRequired { get; set; }
        public float Strength { get; set; }
        public float BaseCapacity { get; set; }
        public float CapacityModifier { get; set; }

        public void InitialiseHarvestSettings(EToolType toolRequired, float strength, float baseCapacity, float capacityModifier)
        {
            ToolRequired = toolRequired;
            Strength = strength;
            BaseCapacity = baseCapacity;
            CapacityModifier = capacityModifier;
        }
    }
}
