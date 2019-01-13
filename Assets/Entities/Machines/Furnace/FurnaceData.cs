using System;
using TheWorkforce.Entities.Views;
using UnityEngine;


namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Furnace Data", menuName = "Entity Data/Machines/Furnace")]
    public class FurnaceData : EntityData
    {
        public FurnaceView View;
        public Sprite Sprite;

        public float HeatRequired = 10.0f;
        public float HeatGenerationRate = 2.0f;
        
        public void InstanceView(FurnaceEntity entity)
        {
            Display();
        }

        public override void Display()
        {
            throw new NotImplementedException();
        }

        public void Display(FurnaceEntity data)
        {
            throw new NotImplementedException();
        }

        public override void Hide()
        {
            throw new NotImplementedException();
        }

        public override GameObject SpawnObject(EntityInstance instance)
        {
            GameObject gameObject = Template();
            gameObject.AddComponent<SpriteRenderer>().sprite = Sprite;

            return gameObject;
        }

        public override EntityInstance CreateInstance(uint id)
        {
            return new FurnaceEntity(id, this);
        }

        public override EntityInstance CreateInstance(uint id, byte[] arr)
        {
            throw new NotImplementedException();
        }
    }
}
