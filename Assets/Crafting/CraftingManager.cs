using System.Collections.Generic;
using TheWorkforce.Items;
using UnityEngine;

namespace TheWorkforce.Crafting
{
    public class CraftingManager
    {
        private readonly List<IItem> _items;
        private readonly CraftingRecipeCollection _craftingRecipes;

        public CraftingManager()
        {
            _items = new List<IItem>();
            _craftingRecipes = new CraftingRecipeCollection();
        }

        public void RegisterItems(IEnumerable<IItem> items)
        {
            _items.AddRange(items);
        }

        public void RegisterRecipes(IEnumerable<CraftingRecipe> recipes)
        {
            foreach (var recipe in recipes)
            {
                _craftingRecipes.AddRecipe(recipe);
            }
        }
    }
}