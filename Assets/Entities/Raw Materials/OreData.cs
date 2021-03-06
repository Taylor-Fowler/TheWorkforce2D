﻿using System;
using UnityEngine;

namespace TheWorkforce.Entities
{
    using Interfaces;

    [CreateAssetMenu(fileName = "Ore Data", menuName = "Entity Data/Raw Materials/Ore")]
    public class OreData : EntityData, ISlotDisplay
    {
        public EntityViewLink ViewLink;
        public Generatable Generatable; // Generation reference that stores how much stone would be generated in a single vein

        public ushort TicksToHarvest => _ticksToHarvest;
        [SerializeField] private ushort _ticksToHarvest = 120;

        public override void Initialise(ushort id)
        {
            base.Initialise(id);
            // Register as a generatable object
            Generatable.Initialise(id);
        }

        public override int PacketSize()
        {
            return base.PacketSize() + sizeof(ushort);
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
            return new OreEntity(id, x, y, onDestroy, this);
        }

        public override EntityInstance CreateInstance(uint id, int x, int y, Action<uint> onDestroy, byte[] arr)
        {
            return new OreEntity(id, x, y, onDestroy, this, BitConverter.ToUInt16(arr, 0));
        }

        public void Display(SlotButton slotButton)
        {
            slotButton.SetItemImage(Sprite);
        }
    }
}
