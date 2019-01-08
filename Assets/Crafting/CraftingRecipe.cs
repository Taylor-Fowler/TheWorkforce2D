using System.Collections.Generic;
using TheWorkforce.Items;

namespace TheWorkforce.Crafting
{
    public class CraftingRecipe
    {
        private static ushort CurrentId = 0;

        public ushort Id { get; }
        public float CraftingTime { get; }
        public List<ushort> ItemsRequired { get; }
        public List<ushort> ItemsProduced { get; }

        private CraftingRecipe()
        {
            Id = ++CurrentId;
            ItemsRequired = new List<ushort>();
            ItemsProduced = new List<ushort>();
        }

        public CraftingRecipe(ushort[][] required, ushort[][] produced, float craftingTime) : this()
        {
            foreach (var requirement in required)
            {
                RegisterRequirement(requirement[0], requirement[1]);
            }

            foreach (var produce in produced)
            {
                RegisterProduce(produce[0], produce[1]);
            }

            CraftingTime = craftingTime;
        }

        public void RegisterRequirement(ushort itemRequiredId, ushort required)
        {
            ItemsRequired.Add(itemRequiredId);
            ItemsRequired.Add(required);
        }

        public void RegisterProduce(ushort itemProducedId, ushort produced)
        {
            ItemsProduced.Add(itemProducedId);
            ItemsProduced.Add(produced);
        }
    }

    public class CraftingRecipeComparer : IEqualityComparer<CraftingRecipe>
    {
        public bool Equals(CraftingRecipe a, CraftingRecipe b)
        {
            return a.Id == b.Id;
        }

        public int GetHashCode(CraftingRecipe obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}