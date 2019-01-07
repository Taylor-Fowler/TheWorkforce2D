namespace TheWorkforce.Items
{
    public interface IHarvestRequirements
    {
        BasicHarvestRequirements BasicHarvestRequirements { get; }        
    }

    public class BasicHarvestRequirements
    {
        private float _harvestSpeed;
        private int _harvestAmount;
        private EToolType _harvestTool;
    }
}