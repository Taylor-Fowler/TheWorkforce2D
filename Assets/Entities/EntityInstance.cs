using System;
using UnityEngine;

namespace TheWorkforce.Entities
{
    public delegate void EntityDestroyHandler();

    public abstract class EntityInstance : IEntityDisplay
    {
        public event Action DirtyHandler;
        public event EntityDestroyHandler OnEntityDestroy;
        public readonly int X;
        public readonly int Y;

        private readonly uint _id;
        private readonly Action<uint> _onDestroy;

        public EntityInstance(uint id, int x, int y, Action<uint> onDestroy)
        {
            _id = id;
            X = x;
            Y = y;
            _onDestroy = onDestroy;
        }

        public abstract byte[] GetPacket();

        public abstract EntityData GetData();
        public abstract uint GetDataTypeId();

        public abstract GameObject Spawn();

        public abstract void Display(EntityView entityView);

        public uint GetId()
        {
            return _id;
        }

        public void Destroy()
        {
            OnEntityDestroy?.Invoke();
            _onDestroy(_id);
        }

        protected void OnDirty()
        {
            Action dirty = DirtyHandler;
            if(dirty != null)
            {
                dirty.Invoke();
            }
        }
    }
}
