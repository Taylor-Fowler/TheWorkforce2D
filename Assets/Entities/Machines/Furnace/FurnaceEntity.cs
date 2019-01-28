using System;
using TheWorkforce.Crafting;
using TheWorkforce.Inventory;
using TheWorkforce.Interfaces;
using TheWorkforce.Scalars;
using UnityEngine;
using TheWorkforce.Entities.Interactions;

namespace TheWorkforce.Entities
{
    public class FurnaceEntity : EntityInstance, IInteract
    {
        private readonly FurnaceData _data;

        // Every EntityInstance has a WorldPosition
        // EntityData will return const int declaring how many bytes are needed

        // Slot = ItemStack = ushort and ushort = 4 bytes
        public readonly ConstrainedSlot<IFuel> FuelSlot; // 4 bytes
        public readonly Slot Input; // 8 bytes
        public readonly OutputSlot Output; // 12 bytes

        public float Heat = 0.0f;
        public Fuel CurrentFuel;
        public float FuelTimeProcessed = 0.0f;
        public float RecipeTimeProcessed = 0.0f;

        public CraftingRecipe CurrentlyProcessing = null;

        private Slot _outputSlot;


        public FurnaceEntity(uint id, int x, int y, Action<uint> onDestroy, FurnaceData data) : base(id, x, y, onDestroy)
        {
            _data = data;

            CurrentFuel = new Fuel(0.0f, 0.0f);

            Input = new Slot();
            FuelSlot = new ConstrainedSlot<IFuel>(new Slot());
            _outputSlot = new Slot();
            Output = new OutputSlot(_outputSlot);
        }

        public FurnaceEntity(uint id, int x, int y, Action<uint> onDestroy, FurnaceData data, byte[] arr) : this(id, x, y, onDestroy, data)
        {

        }

        public override uint GetDataTypeId()
        {
            return _data.Id;
        }

        public override GameObject Spawn()
        {
            return _data.Template();
        }

        public override void Display(EntityView entityView)
        {
            _data.Display(entityView);
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

        public Interaction Interact(EntityInstance initiator)
        {
            throw new NotImplementedException();
        }

        public override EntityData GetData()
        {
            return _data;
        }
    }
}
