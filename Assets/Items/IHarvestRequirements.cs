
namespace TheWorkforce
{
    public interface IHarvestRequirements
    {
        EToolType HarvestTool { get; }
        float HarvestSpeed { get; }
        int HarvestAmount { get; }


        void InitialiseHarvestRequirements(EToolType harvestTool, float harvestSpeed, int harvestAmount);
        //void Harvest(EToolType tool);        
    }
}