using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Tree Data", menuName = "Entity Data/Raw Materials/Tree")]
    public class TreeData : EntityData
    {
        public EntityViewLink ViewLink;
        public Generatable Generatable;
        
        public WoodenLogData Drop => _drop;
        [SerializeField] private WoodenLogData _drop;

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
            return new TreeEntity(id, x, y, onDestroy, this);
        }

        public override EntityInstance CreateInstance(uint id, int x, int y, Action<uint> onDestroy, byte[] arr)
        {
            return new TreeEntity(id, x, y, onDestroy, this, BitConverter.ToUInt16(arr, 0));
        }
    }

}