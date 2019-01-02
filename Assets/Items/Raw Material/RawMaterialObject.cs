using UnityEngine;

namespace TheWorkforce.Items
{
    public class RawMaterialObject : ItemObject
    {
        public float AmountLeft { get; private set; }


        public RawMaterialObject(RawMaterial settings, float amount) : base(settings)
        {
            AmountLeft = amount;
        }

        public override ItemController GetGameObject(Transform parent)
        {
            GameObject gameObject = InitialiseObject(parent);
            var controller = gameObject.AddComponent<ItemController>();
            controller.SetItem(Item);
            return controller;
        }

        public override void Display(ItemInspector itemInspector)
        {
        }

        public override object[] Pack()
        {
            return new object[2] { Item.Id, AmountLeft};
        }

        //public static ItemObject Load(ItemManager itemManager, params object[] data)
        //{

        //    // Find the raw material settings in the item manager, use it to setup any base stuff
        //    return null;
        //}
    }
}
