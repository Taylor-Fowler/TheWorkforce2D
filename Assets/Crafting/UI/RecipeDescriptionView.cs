using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheWorkforce.Crafting
{
    public class RecipeDescriptionView : MonoBehaviour
    {
        [SerializeField] private Image _produceImage;
        [SerializeField] private Transform _ingredientRowPrefab;
        [SerializeField] private TextMeshProUGUI _ingredientCountPrefab;
        [SerializeField] private Image _ingredientImagePrefab;
        [SerializeField] private TextMeshProUGUI _ingredientNamePrefab;
        private List<Transform> _ingredientDisplay;

        public void Display(CraftingRecipe recipe)
        {
            if(_ingredientDisplay == null)
            {
                _ingredientDisplay = new List<Transform>();
            }
            else
            {
                foreach(var display in _ingredientDisplay)
                {
                    Destroy(display.gameObject);
                }
                _ingredientDisplay.Clear();
            }

            _produceImage.sprite = recipe.ItemProduced.Item.Sprite;

            foreach(var ingredient in recipe.Ingredients)
            {
                Transform row = Instantiate(_ingredientRowPrefab, transform);
                _ingredientDisplay.Add(row);

                TextMeshProUGUI ingredientName = Instantiate(_ingredientNamePrefab, row.transform);
                ingredientName.text = ingredient.Item.Name;

                Image ingredientImage = Instantiate(_ingredientImagePrefab, row.transform);
                ingredientImage.sprite = ingredient.Item.Sprite;

                TextMeshProUGUI ingredientCount = Instantiate(_ingredientCountPrefab, row.transform);
                ingredientCount.text = ingredient.Count.ToString() + "x";
            }
        }
    }

}