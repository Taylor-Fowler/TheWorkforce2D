namespace TheWorkforce.Items
{
    public interface IHarvestable
    {
        IHarvestSettings HarvestSettings { get; }
        void SetHarvestSettings(IHarvestSettings harvestSettings);
    }
}
