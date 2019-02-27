using UnityEngine;
using UnityEngine.UI;

namespace TheWorkforce.Crafting
{
    using UI;
    using Interfaces;

    public class PlayerRecipeQueueDisplay : MonoBehaviour, IDisplay
    {
        [SerializeField] private Image _recipeImage;
        [SerializeField] private Image _fillImage;
        private FillTransition _fillTransition;

        private void Awake()
        {
            _fillTransition = new FillTransition();
        }

        public void Display()
        {
            // here is where to expand the recipe queue
        }

        public void Hide()
        {
            // here is where to contract the recipe queue
        }

        public void Listen(RecipeProcessorQueue recipeProcessorQueue)
        {
            recipeProcessorQueue.OnStartProcess += RecipeProcessorQueue_OnStartProcess;
            recipeProcessorQueue.OnProcessing += RecipeProcessorQueue_OnProcessing;
            recipeProcessorQueue.OnFinishedProcess += RecipeProcessorQueue_OnFinishedProcess;
        }

        private void RecipeProcessorQueue_OnStartProcess(CraftingRecipe recipeStarted)
        {
            _recipeImage.enabled = true;
            _fillImage.enabled = true;
            _recipeImage.sprite = recipeStarted.ItemProduced.Item.Sprite;   
            // TODO: Add a reference to the FPS, or move all timing based stuff over to using the game timer
            _fillTransition.ManualTransition(_fillImage, recipeStarted.CraftingTime / 60.0f, 0.0f);
        }

        private void RecipeProcessorQueue_OnFinishedProcess(ItemStack processedItem)
        {
            _recipeImage.enabled = false;
            _fillImage.enabled = false;
        }

        private void RecipeProcessorQueue_OnProcessing(RecipeProcessor processor)
        {
            _fillTransition.ManualTransition(_fillImage, processor.Processing.CraftingTime / 60.0f, processor.TimeProcessed / 60.0f);
        }
    }

}