﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Crafting
{
    using Game_State; using Inventory; using SOs.References;

    public class PlayerCrafting : MonoBehaviour
    {
        // Crafting order
        // 1: Click on the crafting recipe icon
        // 2: Check if there are enough items in the inventory
        // 3: Remove the items from the inventory
        // 4: Begin processing
        // 5: At the end of the process, add the resulting item to the inventory

        [SerializeField] private PlayerCraftingDisplayRef _playerCraftingDisplayRef;
        [SerializeField] private PlayerRecipeQueueDisplayRef _playerRecipeQueueDisplayRef;

        private PlayerCraftingDisplay _playerCraftingDisplay;
        private PlayerRecipeQueueDisplay _playerRecipeQueueDisplay;
        private RecipeProcessorQueue _recipeProcessorQueue;
        private SlotCollection _inventory;

        public void Initialise(SlotCollection inventory)
        {

            _playerCraftingDisplay = _playerCraftingDisplayRef.Get();
            _playerRecipeQueueDisplay = _playerRecipeQueueDisplayRef.Get();
            _inventory = inventory;
            _recipeProcessorQueue = new RecipeProcessorQueue();

            _playerRecipeQueueDisplay.Listen(_recipeProcessorQueue);
            _playerCraftingDisplay.CanCraftRecipe = TryCraft;
            _recipeProcessorQueue.UnloadProduce = _inventory.TransactionalAdd;
        }

        private bool TryCraft(CraftingRecipe recipe)
        {
            bool result = _inventory.TransactionalRemove(recipe.Ingredients);
            if(result)
            {
                _recipeProcessorQueue.AddProcess(recipe);
            }
            return result;
        }
    }
}