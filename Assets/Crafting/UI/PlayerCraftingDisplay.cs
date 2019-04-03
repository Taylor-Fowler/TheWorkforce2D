using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheWorkforce.Crafting
{
    using SOs.Registers;
    using UI;

    [RequireComponent(typeof(RegisterPlayerCraftingDisplay))]
    public class PlayerCraftingDisplay : MonoBehaviour, IDisplay
    {
        public Func<CraftingRecipe, bool> CanCraftRecipe;

        [SerializeField] private Canvas _canvas;
        [SerializeField] private Recipes _recipes;
        [SerializeField] private Button _categoryButton;
        [SerializeField] private CraftItemButton _craftItemPrefab;

        // Transforms just used as a reference point to instantiate
        [SerializeField] private Transform _craftingButtonsAnchor;
        [SerializeField] private RecipeDescriptionView _recipeDescriptionView;

        // take a action reference to the player inventory...this display breaks down
        // the recipe into a list of the items required and sends it to the inventory
        // player must have a crafting processor which sends the currently crafting item to
        // the HUD displaying what is being produced. also creates the coroutine which eventually
        // adds the new item to the inventory
        // if the inventory is full, the crafting queue is paused

        private CraftItemButton _currentlyInspecting;

        // Categories for each type of item
        // Crafting component...
        // Machiness

        private List<CraftItemButton> _allButtons;

        private void Awake()
        {
            Hide();
            _recipes.OnInitialised += this._recipes_OnInitialised;
        }

        private void _recipes_OnInitialised()
        {
            // generate the buttons
            foreach(var recipe in _recipes.AllRecipes)
            {
                CraftItemButton button = Instantiate(_craftItemPrefab, _craftingButtonsAnchor);
                button.OnCraft = TryCraft;
                button.OnInspect = Inspect;
                button.AttachRecipe(recipe);
            }
        }

        private void TryCraft(CraftingRecipe recipe)
        {
            if(CanCraftRecipe(recipe))
            {
                // some kind of visual feedback on the UI to show that it was queued
            }
        }

        private void Inspect(CraftingRecipe recipe, CraftItemButton button)
        {
            _currentlyInspecting?.Default();
            _currentlyInspecting = button;

            if(_currentlyInspecting != null)
            {
                _recipeDescriptionView.Display(recipe);
            }
        }

        #region IDisplay Implementation
        public void Display()
        {
            _canvas.enabled = true;
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }
        #endregion
    }
}