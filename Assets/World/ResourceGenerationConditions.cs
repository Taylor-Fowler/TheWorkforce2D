using TheWorkforce.Items;

namespace TheWorkforce.World
{
    public struct ResourceGenerationConditions
    {
        public RawMaterial RawMaterial { get; private set; }

        public ResourceGenerationConditions(RawMaterial rawMaterial)
        {
            RawMaterial = rawMaterial;
        }
    }
}