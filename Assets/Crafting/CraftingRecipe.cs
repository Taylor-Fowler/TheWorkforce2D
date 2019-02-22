using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Crafting
{
    [CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Scriptable Objects/Crafting/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        public ushort Id { get; private set;  }
        public float CraftingTime => _craftingTime;

        [SerializeField] private float _craftingTime;
        [SerializeField] private EditorItemStack[] _itemsRequired;
        [SerializeField] private EditorItemStack[] _itemsProduced;

        public void Initialise(ushort id, Recipes allRecipes)
        {
            Id = id;
            allRecipes.RegisterProduce(_itemsProduced, this);
            allRecipes.RegisterRequirements(_itemsRequired, this);
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