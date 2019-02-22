using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Crafting
{
    [CreateAssetMenu(fileName = "New Recipes List", menuName = "Scriptable Objects/Crafting/Recipes")]
    public class Recipes : ScriptableObject
    {
        public static Recipes Instance => _instance;
        private static Recipes _instance;

        [SerializeField] private CraftingRecipe[] _allRecipes;
        private ushort _currentId = 0;

        private Dictionary<ushort, List<CraftingRecipe>> _producedInsideOf;
        private Dictionary<ushort, List<CraftingRecipe>> _producedItemRecipes;
        private Dictionary<ushort, List<CraftingRecipe>> _requiredItemRecipes;

        /// <summary>
        /// Initialises the lists required for managing all of the crafting recipes and then
        /// proceeds to initialise the attached recipes by giving them an Id managed by this object
        /// </summary>
        public void Initialise()
        {
            if(_instance == null)
            {
                _instance = this;
            }
            _producedInsideOf = new Dictionary<ushort, List<CraftingRecipe>>();
            _producedItemRecipes = new Dictionary<ushort, List<CraftingRecipe>>();
            _requiredItemRecipes = new Dictionary<ushort, List<CraftingRecipe>>();

            foreach(var recipe in _allRecipes)
            {
                recipe.Initialise(++_currentId, this);
            }
        }

        /// <summary>
        /// Clears the singleton reference and resets the Id counter
        /// </summary>
        public void Clear()
        {
            _currentId = 0;
            _instance = null;
        }

        public CraftingRecipe Get(ushort ingredientId, ushort insideId)
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

        public void RegisterProduce(EditorItemStack[] itemsProduced, CraftingRecipe recipe)
        {
            foreach(EditorItemStack itemStack in itemsProduced)
            {
                AddToDictionary(_producedItemRecipes, itemStack.Item.Id, recipe);
            }
        }

        public void RegisterRequirements(EditorItemStack[] itemsRequired, CraftingRecipe recipe)
        {
            foreach (EditorItemStack itemStack in itemsRequired)
            {
                AddToDictionary(_requiredItemRecipes, itemStack.Item.Id, recipe);
            }
        }

        public void RegisterProducedInside(ushort machineId, CraftingRecipe recipe)
        {
            AddToDictionary(_producedInsideOf, machineId, recipe);
        }

        private void AddToDictionary(Dictionary<ushort, List<CraftingRecipe>> dictionary, ushort itemId, CraftingRecipe recipe)
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
