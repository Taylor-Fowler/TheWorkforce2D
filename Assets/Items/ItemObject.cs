using System.Collections.Generic;
using UnityEngine;
using TheWorkforce.UI;

namespace TheWorkforce.Items
{
    public abstract class ItemObject : IMouseOver, IMouseExit
    {
        #region Public Properties
        public IItem Item { get; private set; }
        #endregion

        #region Private Members
        private List<IMouseOver> _mouseOverBehaviours;
        private List<IMouseExit> _mouseExitBehaviours;
        #endregion

        #region Conversion Operators
        public static implicit operator EItemType(ItemObject itemObject)
        {
            return itemObject.Item.ItemType;
        }
        #endregion

        #region Constructors
        public ItemObject(IItem item)
        {
            Item = item;
        }
        #endregion

        #region Abstract Methods
        public abstract ItemController GetGameObject(Transform parent);
        public abstract void Display(ItemInspector itemInspector);
        public abstract object[] Pack();
        // public abstract ItemObject Load(ItemManager itemManager, params object[] data);
        // TODO: Abstract save
        #endregion

        public virtual void MouseOver()
        {
            foreach(var mouseOver in _mouseOverBehaviours)
            {
                mouseOver.MouseOver();
            }
        }

        public virtual void MouseExit()
        {
            foreach(var mouseExit in _mouseExitBehaviours)
            {
                mouseExit.MouseExit();
            }
        }

        protected virtual GameObject InitialiseObject(Transform parent)
        {
            GameObject spawned = new GameObject(Item.Name);
            spawned.transform.SetParent(parent);

            return spawned;
        }
    }

    #region Generation Interfaces
    //public interface IGenerationSettings
    //{
    //    float MaximumMoisture { get; }
    //    float MinimumMoisture { get; }

    //    float MaximumElevation { get; }
    //    float MinimumElevation { get; }
    //    void InitialiseGenerationSettings(float maxMoisture, float minMoisture, float maxElevation, float minElevation);
    //    bool CanGenerate(float moisture, float elevation);
    //}

    //public interface IGeneratable
    //{
    //    IGenerationSettings GenerationSettings { get; }

    //    void SetGenerationSettings(IGenerationSettings generationSettings);
    //    ItemObject Generate(float moisture, float elevation);
    //}

    //public class BaseGenerationSettings : IGenerationSettings
    //{
    //    #region IGeneratable Members
    //    public float MaximumMoisture { get; private set; }
    //    public float MinimumMoisture { get; private set; }
    //    public float MaximumElevation { get; private set; }
    //    public float MinimumElevation { get; private set; }

    //    public void InitialiseGenerationSettings(float maxMoisture, float minMoisture, float maxElevation, float minElevation)
    //    {
    //        MaximumMoisture = maxMoisture;
    //        MinimumMoisture = minMoisture;
    //        MaximumElevation = maxElevation;
    //        MinimumElevation = minElevation;
    //    }

    //    public bool CanGenerate(float moisture, float elevation)
    //    {
    //        return moisture >= MinimumMoisture
    //            && moisture <= MaximumMoisture
    //            && elevation >= MinimumElevation
    //            && elevation <= MaximumElevation;
    //    }
    //    #endregion
    //}
    #endregion

    #region Harvest Interfaces
    //public interface IHarvestSettings
    //{
    //    EToolType ToolRequired { get; }
    //    float Strength { get; }
    //    float BaseCapacity { get; }
    //    float CapacityModifier { get; }

    //    void InitialiseHarvestSettings(EToolType toolRequired, float strength, float baseCapacity, float capacityModifier);
    //}

    //public interface IHarvestable
    //{
    //    IHarvestSettings HarvestSettings { get; }
    //    void SetHarvestSettings(IHarvestSettings harvestSettings);
    //}

    //public class BaseHarvestSettings : IHarvestSettings
    //{
    //    public EToolType ToolRequired { get; private set; }
    //    public float Strength { get; private set; }
    //    public float BaseCapacity { get; private set; }
    //    public float CapacityModifier { get; private set; }

    //    public void InitialiseHarvestSettings(EToolType toolRequired, float strength, float baseCapacity, float capacityModifier)
    //    {
    //        ToolRequired = toolRequired;
    //        Strength = strength;
    //        BaseCapacity = baseCapacity;
    //        CapacityModifier = capacityModifier;
    //    }
    //}
    #endregion

    //public abstract class ItemSettings : IItem
    //{
    //    public int Id { get; private set; }
    //    public string Name { get; private set; }
    //    public string Description { get; private set; }
    //    public EItemType ItemType { get; private set; }
    //    public Sprite Icon { get; private set; }
    //    public int MaxStackSize { get; private set; }

    //    public void InitialiseItem(int id, string name, string description, int itemType, Sprite icon, int maxStackSize = 1)
    //    {
    //        Id = id;
    //        Name = name;
    //        Description = description;
    //        ItemType = (EItemType)itemType;
    //        Icon = icon;
    //        MaxStackSize = maxStackSize;
    //    }

    //    //public ItemController SpawnObject(Transform parent)
    //    //{
    //    //    return null;
    //    //}
    //}

    //public class RawMaterialSettings : ItemSettings, IHarvestable, IGeneratable
    //{
    //    #region IGeneratable
    //    public IGenerationSettings GenerationSettings { get; private set; }

    //    public void SetGenerationSettings(IGenerationSettings generationSettings)
    //    {
    //        GenerationSettings = generationSettings;
    //    }

    //    public virtual ItemObject Generate(float moisture, float elevation)
    //    {
    //        if(GenerationSettings.CanGenerate(moisture, elevation))
    //        {
    //            return new RawMaterialObject(this, HarvestSettings.BaseCapacity);
    //        }
    //        return null;
    //    }
    //    #endregion

    //    #region IHarvestable
    //    public IHarvestSettings HarvestSettings { get; private set; }

    //    public void SetHarvestSettings(IHarvestSettings harvestSettings)
    //    {
    //        HarvestSettings = harvestSettings;
    //    }
    //    #endregion
    //}



    //public class RawMaterialObject : ItemObject
    //{
    //    public float AmountLeft { get; private set; }


    //    public RawMaterialObject(RawMaterialSettings settings, float amount) : base(settings)
    //    {
    //        AmountLeft = amount;
    //    }

    //    public override ItemController GetObject(Transform parent)
    //    {
    //        GameObject gameObject = InitialiseObject(parent);
    //        var controller = gameObject.AddComponent<ItemController>();
    //        controller.SetItem(Item);
    //        return controller;
    //    }

    //    public override void Display(ItemInspector itemInspector)
    //    {
    //    }

    //    //public static ItemObject Load(ItemManager itemManager, params object[] data)
    //    //{

    //    //    // Find the raw material settings in the item manager, use it to setup any base stuff
    //    //    return null;
    //    //}
    //}
}
