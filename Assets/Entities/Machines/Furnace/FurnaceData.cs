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

        public override int PacketSize()
        {
            return base.PacketSize();
        }

        public override void Display(EntityView entityView)
        {
            entityView.SetTitle(Name);
            entityView.SetDescription(Description);
            entityView.SetImage(Sprite);
        }

        public override GameObject Template()
        {
            GameObject gameObject = base.Template();
            gameObject.AddComponent<SpriteRenderer>().sprite = Sprite;

            return gameObject;
        }

        public override EntityInstance CreateInstance(uint id, int x, int y, Action<uint> onDestroy)
        {
            return new FurnaceEntity(id, x, y, onDestroy, this);
        }

        public override EntityInstance CreateInstance(uint id, int x, int y, Action<uint> onDestroy, byte[] arr)
        {
            return new FurnaceEntity(id, x, y, onDestroy, this, arr);
        }

        public void Display(SlotButton slotButton)
        {
            slotButton.SetItemImage(Sprite);
        }
    }
}
