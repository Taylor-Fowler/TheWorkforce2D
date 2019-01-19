﻿using System;
using TheWorkforce.Entities.Views;
using UnityEngine;


namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Furnace Data", menuName = "Entity Data/Machines/Furnace")]
    public class FurnaceData : EntityData
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

        public override EntityInstance CreateInstance(uint id)
        {
            return new FurnaceEntity(id, this);
        }

        public override EntityInstance CreateInstance(uint id, byte[] arr)
        {
            return new FurnaceEntity(id, this, arr);
        }
    }
}
