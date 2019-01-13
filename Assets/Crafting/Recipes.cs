using System.Linq;
using System.Collections.Generic;
using TheWorkforce.Items;

namespace TheWorkforce.Crafting
{
    public static class Recipes
    {
        private static Dictionary<ushort, List<CraftingRecipe>> _producedInsideOf;
        private static Dictionary<ushort, List<CraftingRecipe>> _producedItemRecipes;
        private static Dictionary<ushort, List<CraftingRecipe>> _requiredItemRecipes;


        public static void Initialise()
        {
            _producedInsideOf = new Dictionary<ushort, List<CraftingRecipe>>();
            _producedItemRecipes = new Dictionary<ushort, List<CraftingRecipe>>();
            _requiredItemRecipes = new Dictionary<ushort, List<CraftingRecipe>>();

            //AddRecipe(new CraftingRecipe
            //    (
            //        new ushort[][]
            //        {
            //            new ushort[] { Stone.ItemData.Id, 5 }
            //        },
            //        new ushort[][]
            //        {
            //            new ushort[] { Furnace.ItemData.Id, 1 }
            //        },
            //        2.0f
            //    )
            //);

            //AddRecipe(new CraftingRecipe
            //    (
            //        new ushort[][]
            //        {
            //            new ushort[] { IronOre.ItemData.Id, 1 }
            //        },
            //        new ushort[][]
            //        {
            //            new ushort[] { IronIngot.ItemData.Id, 1 }
            //        },
            //        5.0f
            //    ),
            //    Furnace.ItemData.Id
            //);
        }

        public static CraftingRecipe Get(ushort ingredientId, ushort insideId)
        {
            List<CraftingRecipe> insideList = null;

            if(_producedInsideOf.TryGetValue(insideId, out insideList))
            {
                List<CraftingRecipe> ingredientList = null;
                if(_requiredItemRecipes.TryGetValue(ingredientId, out ingredientList))
                {
                    var intersected = insideList.Intersect(ingredientList, new CraftingRecipeComparer());
                    return intersected.GetEnumerator().Current;
                }
            }
            return null;
        }

        private static void AddRecipe(CraftingRecipe recipe)
        {
            AddRecipe(recipe, 0);
        }

        private static void AddRecipe(CraftingRecipe recipe, ushort insideId)
        {
            AddToDictionary(_producedInsideOf, insideId, recipe);
            RegisterProduce(recipe);
            RegisterRequirements(recipe);
        }

        private static void RegisterProduce(CraftingRecipe recipe)
        {
            for(int i = 0; i < recipe.ItemsProduced.Count; i += 2)
            {
                AddToDictionary(_producedItemRecipes, recipe.ItemsProduced[i], recipe);
            }
        }

        private static void RegisterRequirements(CraftingRecipe recipe)
        {
            for (int i = 0; i < recipe.ItemsRequired.Count; i += 2)
            {
                AddToDictionary(_requiredItemRecipes, recipe.ItemsRequired[i], recipe);
            }
        }

        private static void AddToDictionary(Dictionary<ushort, List<CraftingRecipe>> dictionary, ushort itemId, CraftingRecipe recipe)
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
