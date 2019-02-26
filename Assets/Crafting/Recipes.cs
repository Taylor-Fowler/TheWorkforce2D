using System;
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

        /// <summary>
        /// Backing field for the OnInitialised event...explicit events require this
        /// </summary>
        private event Action _onInitialised;
        public event Action OnInitialised
        {
            add
            {
                if(_isInitialised)
                {
                    value.Invoke();
                }
                else
                {
                    _onInitialised += value;
                }
            }
            remove
            {
                _onInitialised -= value;
            }
        }

        public CraftingRecipe[] AllRecipes => _allRecipes;
        [SerializeField] private CraftingRecipe[] _allRecipes;
        private bool _isInitialised = false;
        private ushort _currentId = 0;

        private Dictionary<ushort, List<CraftingRecipe>> _producedInsideOf;
        private Dictionary<ushort, List<CraftingRecipe>> _produceRecipes;
        private Dictionary<ushort, List<CraftingRecipe>> _ingredientsRecipes;

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
            _produceRecipes = new Dictionary<ushort, List<CraftingRecipe>>();
            _ingredientsRecipes = new Dictionary<ushort, List<CraftingRecipe>>();

            foreach(var recipe in _allRecipes)
            {
                recipe.Initialise(++_currentId, this);
            }

            _isInitialised = true;
            Action onInitialisation = _onInitialised;
            onInitialisation?.Invoke();
        }

        /// <summary>
        /// Clears the singleton reference and resets the Id counter
        /// </summary>
        public void Clear()
        {
            _currentId = 0;
            _instance = null;
            _isInitialised = false;
        }

        public CraftingRecipe Get(ushort ingredientId, ushort insideId)
        {
            List<CraftingRecipe> insideList = null;

            if(_producedInsideOf.TryGetValue(insideId, out insideList))
            {
                List<CraftingRecipe> ingredientList = null;
                if(_ingredientsRecipes.TryGetValue(ingredientId, out ingredientList))
                {
                    var intersected = insideList.Intersect(ingredientList, new CraftingRecipeComparer());
                    return intersected.GetEnumerator().Current;
                }
            }
            return null;
        }

        public void RegisterProduce(EditorItemStack itemProduced, CraftingRecipe recipe)
        {
            AddToDictionary(_produceRecipes, itemProduced.Item.Id, recipe);
        }

        public void RegisterIngredients(EditorItemStack[] ingredients, CraftingRecipe recipe)
        {
            foreach (EditorItemStack itemStack in ingredients)
            {
                AddToDictionary(_ingredientsRecipes, itemStack.Item.Id, recipe);
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
