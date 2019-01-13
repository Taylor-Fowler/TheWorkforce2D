using UnityEngine;

namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Stone Data", menuName = "Entity Data/Raw Materials/Stone")]
    public class StoneData : EntityData
    {
        public Sprite Sprite;
        // Generation reference that stores how much stone would be generated in a single vein

        public override void Initialise(ushort id)
        {
            base.Initialise(id);
            // Register as a generatable object
        }

        public override void Display()
        {
            throw new System.NotImplementedException();
        }

        public override void Hide()
        {
            throw new System.NotImplementedException();
        }

        public override GameObject SpawnObject(EntityInstance instance)
        {
            GameObject gameObject = Template();
            gameObject.AddComponent<SpriteRenderer>().sprite = Sprite;

            return gameObject;
        }

        public override EntityInstance CreateInstance(uint id)
        {
            return new StoneEntity(id, this);
        }

        public override EntityInstance CreateInstance(uint id, byte[] arr)
        {
            throw new System.NotImplementedException();
        }
    }
}
