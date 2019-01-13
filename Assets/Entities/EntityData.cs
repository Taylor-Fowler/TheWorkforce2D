using TheWorkforce.UI;
using UnityEngine;

namespace TheWorkforce.Entities
{
    public abstract class EntityData : ScriptableObject, IDisplay
    {
        public ushort Id;
        public string Name;
        public string Description;
        public byte MaxStackSize = 32;
        public byte Width = 1;
        public byte Height = 1;

        public virtual void Initialise(ushort id)
        {
            Id = id;
        }

        public abstract void Display();
        public abstract void Hide();


        /// <summary>
        /// Create Instance is called when an entity is added to the game, this can be through one of the following ways:
        ///     1. A new chunk is generated
        ///     2. World data is loaded
        ///     3. A player places an item in the world
        /// </summary>
        public abstract EntityInstance CreateInstance(uint id);

        /// <summary>
        /// Create Instance is called when an entity is added to the game, this can be through one of the following ways:
        ///     1. A new chunk is generated
        ///     2. World data is loaded
        ///     3. A player places an item in the world
        /// </summary>
        /// <param name="arr"></param>
        public abstract EntityInstance CreateInstance(uint id, byte[] arr);


        public abstract GameObject SpawnObject(EntityInstance instance);

        protected GameObject Template()
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.localScale = new Vector3(Width, Height, 1.0f);
            gameObject.AddComponent<BoxCollider2D>();

            return gameObject;
        }
    }
}