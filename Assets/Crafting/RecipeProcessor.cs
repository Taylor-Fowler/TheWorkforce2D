using System;
using System.Collections.Generic;

namespace TheWorkforce.Crafting
{
    using Game_State;

    public class RecipeProcessor : TickAction
    {
        public Func<ItemStack, bool> UnloadProduce;

        public event Action<CraftingRecipe> OnStartProcess;
        public event Action<RecipeProcessor> OnProcessing;
        public event Action<ItemStack> OnFinishedProcess;

        public CraftingRecipe Processing => _processing; 
        protected CraftingRecipe _processing;

        public uint TimeProcessed => _timeProcessed;
        private uint _timeProcessed;

        public bool IsProcessing => _processing != null;

        /// <summary>
        /// The amount of time required to process the current recipe
        /// </summary>
        public uint TimeRequired => (_processing != null) ? _processing.CraftingTime : 0;

        private ItemStack _tryingToUnload = null;

        public override void Execute()
        {
            if(_tryingToUnload != null && UnloadProduce(_tryingToUnload))
            {
                SuccessfullyUnloaded();
            }

            if(IsProcessing)
            {
                bool finished = JustFinishedProcess();
                OnProcessing?.Invoke(this);

                if (finished)
                {
                    _tryingToUnload = new ItemStack(_processing.ItemProduced.Item, _processing.ItemProduced.Count);
                    if (UnloadProduce(_tryingToUnload))
                    {
                        SuccessfullyUnloaded();
                    } 
                }
            }
        }

        public virtual void AddProcess(CraftingRecipe recipe)
        {
            // Assign the new recipe and tell listeners about it
            _processing = recipe;
            OnStartProcess?.Invoke(recipe);
        }

        public virtual void CancelProcess()
        {
            _processing = null;
            ResetTimer();
        }

        /// <summary>
        /// Reset the time processed of the recipe to 0
        /// </summary>
        protected void ResetTimer() => _timeProcessed = 0;
        protected bool JustFinishedProcess() => ++_timeProcessed == TimeRequired;

        protected virtual void SuccessfullyUnloaded()
        {
            OnFinishedProcess?.Invoke(_tryingToUnload);
            _tryingToUnload = null;
            _processing = null;
            ResetTimer();
        }
    }

    public class RecipeProcessorQueue : RecipeProcessor
    {
        private Queue<CraftingRecipe> _recipeQueue;

        public RecipeProcessorQueue()
        {
            _recipeQueue = new Queue<CraftingRecipe>();
        }

        public override void AddProcess(CraftingRecipe recipe)
        {
            if(!IsProcessing)
            {
                base.AddProcess(recipe);
            }
            else
            {
                _recipeQueue.Enqueue(recipe);
            }
        }

        public override void CancelProcess()
        {
            base.CancelProcess();
            ProcessQueue();
        }

        protected override void SuccessfullyUnloaded()
        {
            base.SuccessfullyUnloaded();
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (_recipeQueue.Count > 0)
            {
                base.AddProcess(_recipeQueue.Dequeue());
            }
        }
    }
}
