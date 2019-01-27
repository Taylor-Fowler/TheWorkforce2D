﻿using System.Collections.Generic;
using TheWorkforce.Game_State;
using UnityEngine;

namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Entity Collection", menuName = "Entity Data/Collection")]
    public class EntityCollection : ScriptableObject
    {
        #region Singleton
        public static EntityCollection Instance()
        {
            return _instance;
        }

        private static EntityCollection _instance;
        #endregion

        public List<EntityData> Collection;
        public Dictionary<ushort, EntityData> DataMappedToId;
        public Dictionary<uint, EntityInstance> InstanceMappedToId;
        public List<EntityInstance> EntitiesToDestroy;

        private ushort _dataIdCounter = 0;
        private uint _entityIdCounter = 0;

        public void Initialise()
        {
            _instance = this;
            DataMappedToId = new Dictionary<ushort, EntityData>();
            InstanceMappedToId = new Dictionary<uint, EntityInstance>();
            EntitiesToDestroy = new List<EntityInstance>();

            foreach(var value in Collection)
            {
                value.Initialise(++_dataIdCounter);
                DataMappedToId.Add(_dataIdCounter, value);
            }

            GameTime.SubscribeToPostUpdate(DestroyEntities);
        }

        public uint CreateEntity(ushort dataIdKey)
        {
            EntityData value = null;
            //Debug.Log("[EntityCollection] - CreateEntity(ushort)");
            if(DataMappedToId.TryGetValue(dataIdKey, out value))
            {
                InstanceMappedToId.Add(++_entityIdCounter, value.CreateInstance(_entityIdCounter, DestroyEntity));
                return _entityIdCounter;
            }
            return 0;
        }

        public EntityInstance GetEntity(uint entityInstanceId)
        {
            EntityInstance value = null;
            InstanceMappedToId.TryGetValue(entityInstanceId, out value);

            return value;
        }


        private void DestroyEntity(uint entityInstanceId)
        {
            Debug.Log("[EntityCollection] - DestroyEntity(uint) \n" 
                    + "entityInstanceId: " + entityInstanceId.ToString());
            var instance = GetEntity(entityInstanceId);

            if(instance != null)
            {
                EntitiesToDestroy.Add(instance);
            }
        }

        private void DestroyEntities()
        {
            foreach (var entity in EntitiesToDestroy)
            {
                InstanceMappedToId.Remove(entity.GetId());
            }
            EntitiesToDestroy.Clear();
        }
    }
}
