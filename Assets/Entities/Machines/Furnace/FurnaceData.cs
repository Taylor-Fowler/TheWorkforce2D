using System;
using TheWorkforce.Interfaces;
using UnityEngine;


namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Furnace Data", menuName = "Entity Data/Machines/Furnace")]
    public class FurnaceData : EntityData, ISlotDisplay
    {
        public EntityViewLink ViewLink;
        public Sprite Sprite;

        public float HeatRequired = 10.0f;
        public float HeatGenerationRate = 2.0f;
        
        public void InstanceView(FurnaceEntity entity)
        {
            Display();
        }

        public override void Display()
        {
            ViewLink.View.SetTitle(Name);
            ViewLink.View.SetDescription(Description);
            ViewLink.View.SetImage(Sprite);
        }

        public override void Hide()
        {
            ViewLink.View.Hide();
        }

        public override GameObject Template()
        {
            GameObject gameObject = base.Template();
            gameObject.AddComponent<SpriteRenderer>().sprite = Sprite;

            return gameObject;
        }

        public override EntityInstance CreateInstance(uint id, Action<uint> onDestroy)
        {
            return new FurnaceEntity(id, onDestroy, this);
        }

        public override EntityInstance CreateInstance(uint id, Action<uint> onDestroy, byte[] arr)
        {
            return new FurnaceEntity(id, onDestroy, this, arr);
        }

        public void Display(SlotButton slotButton)
        {
            slotButton.SetItemImage(Sprite);
        }
    }
}
