﻿using System;
using UnityEngine;

namespace TheWorkforce.Entities
{
    using Interfaces;

    [Serializable]
    public abstract class EntityInstance : IEntityDisplay
    {
        public event Action DirtyHandler;
        public event Action OnEntityDestroy;
        public readonly int X;
        public readonly int Y;
        public readonly uint Id;

        private readonly Action<uint> _onUnload;

        public EntityInstance(uint id, int x, int y, Action<uint> onUnload)
        {
            Id = id;
            X = x;
            Y = y;
            _onUnload = onUnload;
        }

        public abstract byte[] GetPacket();

        public abstract byte[] GetSaveData();

        public abstract EntityData GetData();
        public abstract uint GetDataTypeId();

        public abstract GameObject Spawn();

        public abstract void Display(EntityView entityView);

        public void Destroy()
        {
            OnEntityDestroy?.Invoke();
            _onUnload(Id);
        }

        public void Unload() => _onUnload?.Invoke(Id);

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
