using System;
using TheWorkforce.Entities.Interactions;
using TheWorkforce.UI;
using UnityEngine;

namespace TheWorkforce.Entities
{
    public delegate void EntityDestroyHandler();

    public abstract class EntityInstance : IDisplay
    {
        public event EntityDestroyHandler OnEntityDestroy;

        private readonly uint _id;
        private readonly Action<uint> _onDestroy;

        public EntityInstance(uint id, Action<uint> onDestroy)
        {
            _id = id;
            _onDestroy = onDestroy;
        }

        public uint GetId()
        {
            return _id;
        }

        public abstract uint GetDataTypeId();
        public abstract GameObject Spawn();
        public abstract void Display();
        public abstract void Hide();

        public void Destroy()
        {
            OnEntityDestroy?.Invoke();
        }
    }
}
