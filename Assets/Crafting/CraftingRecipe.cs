using System.Collections.Generic;
using TheWorkforce.Items;

namespace TheWorkforce.Crafting
{
    public class CraftingRecipe
    {
        public int Id { get; }
        public Dictionary<int, int> ItemsRequired { get; }
        public Dictionary<int, int> ItemsProduced { get; }

        public CraftingRecipe(int id)
        {
            Id = id;
            ItemsRequired = new Dictionary<int, int>();
            ItemsProduced = new Dictionary<int, int>();
        }

        public CraftingRecipe(int id, int[][] required, int[][] produced) : this(id)
        {
            foreach (var requirement in required)
            {
                RegisterRequirement(requirement[0], requirement[1]);
            }

            foreach (var produce in produced)
            {
                RegisterProduce(produce[0], produce[1]);
            }
        }

        public void RegisterRequirement(int itemRequiredId, int required)
        {
            ItemsRequired.Add(itemRequiredId, required);
        }

        public void RegisterProduce(int itemProducedId, int produced)
        {
            ItemsProduced.Add(itemProducedId, produced);
        }
    }
}