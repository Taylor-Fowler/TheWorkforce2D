using TheWorkforce.UI;
using UnityEngine;

namespace TheWorkforce.Entities
{
    public abstract class EntityInstance : IDisplay
    {
        private readonly uint _id;

        public EntityInstance(uint id)
        {
            _id = id;
        }

        public uint GetId()
        {
            return _id;
        }

        public abstract GameObject Spawn();
        public abstract void Display();
        public abstract void Hide();
    }
}
