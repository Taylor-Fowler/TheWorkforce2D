using System;
using TheWorkforce.Crafting;
using TheWorkforce.Inventory;
using TheWorkforce.Interfaces;
using TheWorkforce.Scalars;
using UnityEngine;

namespace TheWorkforce.Entities
{
    public class FurnaceEntity : EntityInstance
    {
        private readonly FurnaceData _data;

        public readonly ConstrainedSlot<IFuel> FuelSlot;
        public readonly Slot Input;
        public readonly OutputSlot Output;

        public float Heat = 0.0f;
        public Fuel CurrentFuel;
        public float FuelTimeProcessed = 0.0f;
        public float RecipeTimeProcessed = 0.0f;

        public CraftingRecipe CurrentlyProcessing = null;

        private Slot _outputSlot;


        public FurnaceEntity(uint id, FurnaceData data) : base(id)
        {
            _data = data;

            CurrentFuel = new Fuel(0.0f, 0.0f);

            Input = new Slot();
            FuelSlot = new ConstrainedSlot<IFuel>(new Slot());
            _outputSlot = new Slot();
            Output = new OutputSlot(_outputSlot);
        }

        public FurnaceEntity(uint id, FurnaceData data, byte[] arr) : this(id, data)
        {

        }

        public override GameObject Spawn()
        {
            return _data.Template();
        }

        public override void Display()
        {
            _data.Display();
        }

        public override void Hide()
        {
            _data.Hide();
        }

        public void ProcessTick(float deltaTime)
        {
            DegradeHeat(deltaTime);
            GenerateHeat(deltaTime);
            ProcessRecipe(deltaTime);
        }

        private void DegradeHeat(float deltaTime)
        {
            Heat -= 0.1f * deltaTime;
            Heat = Math.Max(Heat, 0.0f);
        }

        private void GenerateHeat(float deltaTime)
        {
            if (Heat < _data.HeatRequired)
            {
                Heat += ConsumeFuel(deltaTime) * _data.HeatGenerationRate;
            }
        }

        private float ConsumeFuel(float deltaTime)
        {
            float fuelConsumed = 0.0f;

            if (CurrentFuel.Value != 0.0f)
            {
                FuelTimeProcessed += deltaTime;
                if (FuelTimeProcessed > CurrentFuel.ConsumptionTime)
                {
                    deltaTime += CurrentFuel.ConsumptionTime - FuelTimeProcessed;
                    FuelTimeProcessed = CurrentFuel.ConsumptionTime;
                }

                fuelConsumed = CurrentFuel.Rate() * deltaTime;
            }

            return fuelConsumed;
        }

        private void ProcessRecipe(float deltaTime)
        {
            if (Heat >= _data.HeatRequired)
            {
                IncrementRecipeProcess(deltaTime);
            }
        }

        private void IncrementRecipeProcess(float deltaTime)
        {
            if (CurrentlyProcessing == null)
            {
                return;
            }

            RecipeTimeProcessed += deltaTime;

            if (RecipeTimeProcessed >= CurrentlyProcessing.CraftingTime)
            {
                deltaTime = RecipeTimeProcessed - CurrentlyProcessing.CraftingTime;
                OutputRecipeProduce();
                ProcessNextRecipe();
                IncrementRecipeProcess(deltaTime);
            }
        }

        private void OutputRecipeProduce()
        {
            //ItemData itemData = ItemFactory.Instance.GetById(CurrentlyProcessing.ItemsProduced[0]);
            //_outputSlot.Add(new ItemStack(itemData, CurrentlyProcessing.ItemsProduced[1]));
        }

        private void ProcessNextRecipe()
        {
            CurrentlyProcessing = null;
            RecipeTimeProcessed = 0.0f;

            if (Input.IsEmpty())
            {
                return;
            }

            ItemStack outputStack = Output.ItemStack;
            ItemStack inputStack = Input.ItemStack;

            CurrentlyProcessing = Recipes.Get(inputStack.Item.Id, _data.Id);
        }
    }
}
