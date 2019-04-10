using System;
using UnityEngine;

namespace TheWorkforce.Entities
{
    using Interfaces;

    [CreateAssetMenu(fileName = "Wooden Log Data", menuName = "Entity Data/Raw Materials/Wooden Log")]
    public class WoodenLogData : EntityData, ISlotDisplay
    {
        public EntityViewLink ViewLink;
        // fuel worth

        public override void Display(EntityView entityView)
        {
            entityView.SetTitle(Name);
            entityView.SetDescription(Description);
            entityView.SetImage(Sprite);
        }

        public void Display(SlotButton slotButton)
        {
            slotButton.SetItemImage(Sprite);
        }

        public override EntityInstance CreateInstance(uint id, int x, int y, Action<uint> onDestroy)
        {
            throw new NotImplementedException();
        }

        public override EntityInstance CreateInstance(uint id, int x, int y, Action<uint> onDestroy, byte[] arr)
        {
            throw new NotImplementedException();
        }
    }

}