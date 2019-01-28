using System;
using TheWorkforce.Interfaces;
using UnityEngine;

namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Stone Data", menuName = "Entity Data/Raw Materials/Stone")]
    public class StoneData : EntityData, ISlotDisplay
    {
        public EntityViewLink ViewLink;
        public Sprite Sprite;
        // Generation reference that stores how much stone would be generated in a single vein
        public Generatable Generatable;

        public override void Initialise(ushort id)
        {
            base.Initialise(id);
            // Register as a generatable object
            Generatable = new Generatable(0.7f, 0.3f, 0.9f, 0.5f, id);
        }

        public override void Display(EntityView entityView)
        {
            entityView.SetTitle(Name);
            entityView.SetDescription(Description);
            entityView.SetImage(Sprite);
        }

        public override void Hide()
        {
            
        }

        public override GameObject Template()
        {
            GameObject gameObject = base.Template();
            gameObject.AddComponent<SpriteRenderer>().sprite = Sprite;

            return gameObject;
        }

        public override EntityInstance CreateInstance(uint id, int x, int y, Action<uint> onDestroy)
        {
            return new StoneEntity(id, x, y, onDestroy, this);
        }

        public override EntityInstance CreateInstance(uint id, int x, int y, Action<uint> onDestroy, byte[] arr)
        {
            throw new System.NotImplementedException();
        }

        public void Display(SlotButton slotButton)
        {
            slotButton.SetItemImage(Sprite);
        }
    }
}
