using TheWorkforce.World;
using UnityEngine;

namespace TheWorkforce.Items
{
    public class RawMaterial : ItemInstance, IHarvestRequirements, IGenerationRequirements
    {
        public BasicHarvestRequirements BasicHarvestRequirements { get; protected set; }
        public BasicGenerationRequirements GenerationRequirements { get; protected set; }




        public RawMaterial(
            ItemDetails itemDetails, 
            BasicGenerationRequirements generationRequirements, 
            ItemManager itemManager)
            : base(itemDetails)
        {
        }

        public override void Spawn(Tile tile)
        {

        }

    }
}
