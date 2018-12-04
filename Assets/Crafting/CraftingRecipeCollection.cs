using System.Collections.Generic;

namespace TheWorkforce.Crafting
{
    public class CraftingRecipeCollection
    {
        private readonly Dictionary<int, List<CraftingRecipe>> _producedItemRecipes;
        private readonly Dictionary<int, List<CraftingRecipe>> _requiredItemRecipes;


        public CraftingRecipeCollection()
        {
            _producedItemRecipes = new Dictionary<int, List<CraftingRecipe>>();
            _requiredItemRecipes = new Dictionary<int, List<CraftingRecipe>>();
        }

        public void AddRecipe(CraftingRecipe recipe)
        {
            RegisterProduce(recipe);
            RegisterRequirements(recipe);
        }


        private void RegisterProduce(CraftingRecipe recipe)
        {
            foreach (var producedItems in recipe.ItemsProduced)
            {
                AddToDictionary(_producedItemRecipes, producedItems.Key, recipe);
            }
        }

        private void RegisterRequirements(CraftingRecipe recipe)
        {
            foreach (var requiredItems in recipe.ItemsRequired)
            {
                AddToDictionary(_requiredItemRecipes, requiredItems.Key, recipe);
            }
        }

        private static void AddToDictionary(Dictionary<int, List<CraftingRecipe>> dictionary, int itemId, CraftingRecipe recipe)
        {
            List<CraftingRecipe> recipesAssociatedWithItem;
            if (!dictionary.TryGetValue(itemId, out recipesAssociatedWithItem))
            {
                recipesAssociatedWithItem = new List<CraftingRecipe>();
                dictionary.Add(itemId, recipesAssociatedWithItem);
            }
            
            recipesAssociatedWithItem.Add(recipe);
        }
    }
}