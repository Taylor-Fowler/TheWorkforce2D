using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Crafting
{
    using Entities;

    [CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Scriptable Objects/Crafting/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        public ushort Id { get; private set; }

        public uint CraftingTime => _craftingTime;
        [SerializeField] private uint _craftingTime;

        public EditorItemStack ItemProduced => _itemProduced;
        [SerializeField] private EditorItemStack _itemProduced;

        public EditorItemStack[] Ingredients => _ingredients;
        [SerializeField] private EditorItemStack[] _ingredients;

        public void Initialise(ushort id, Recipes allRecipes)
        {
            Id = id;
            allRecipes.RegisterProduce(_itemProduced, this);
            allRecipes.RegisterIngredients(_ingredients, this);
            allRecipes.RegisterProducedInside(0, this);
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