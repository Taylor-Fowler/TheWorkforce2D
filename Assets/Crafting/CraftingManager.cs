using System.Collections.Generic;
using UnityEngine;
using TheWorkforce.Game_State;

namespace TheWorkforce.Crafting
{
    public class CraftingManager : IManager
    {
        #region IManager Implementation
        public GameManager GameManager { get; private set; }

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
        }
        #endregion

        #region Private Members
        private readonly CraftingRecipeCollection _craftingRecipes;
        #endregion

        public CraftingManager()
        {
            _craftingRecipes = new CraftingRecipeCollection();
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