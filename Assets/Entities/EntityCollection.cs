using System;
using System.Collections.Generic;
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
        public List<EntityInstance> ActiveEntities;
        public Dictionary<ushort, EntityData> DataMappedToId;
        public Dictionary<uint, EntityInstance> InstanceMappedToId;
        public List<EntityInstance> EntitiesToDestroy;

        private ushort _dataIdCounter;
        private uint _entityIdCounter;

        public void Initialise()
        {
            _entityIdCounter = _dataIdCounter = 0;
            //Debug.Log("[EntityCollection] - Initialise() \n" 
            //        + "_dataIdCounter: " + _dataIdCounter.ToString());

            _instance = this;
            ActiveEntities = new List<EntityInstance>();
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

        public uint CreateEntity(ushort dataIdKey, int x, int y)
        {
            //Debug.Log("[EntityCollection] - CreateEntity(ushort)");
            EntityData value = null;
            if(DataMappedToId.TryGetValue(dataIdKey, out value))
            {
                _entityIdCounter++;

                EntityInstance instance = value.CreateInstance(_entityIdCounter, x, y, DestroyEntity);
                ActiveEntities.Add(instance);
                InstanceMappedToId.Add(_entityIdCounter, instance);

                //Debug.Log("[EntityCollection] - CreateEntity(ushort, int, int) \n" +
                //            "X: " + x.ToString() + ", Y: " + y.ToString());
                return _entityIdCounter;
            }
            return 0;
        }

        public void LoadEntity(ushort dataIdKey, uint entityId, int x, int y, byte[] saveData, int offset)
        {
            EntityData value = null;
            if (DataMappedToId.TryGetValue(dataIdKey, out value))
            {
                byte[] entityData = new byte[saveData.Length - offset];
                Array.Copy(saveData, offset, entityData, 0, entityData.Length);

                EntityInstance entityInstance = value.CreateInstance(entityId, x, y, DestroyEntity, entityData);
                ActiveEntities.Add(entityInstance);
                InstanceMappedToId.Add(entityId, entityInstance);

                Debug.Log("[EntityCollection] - LoadEntity(ushort, uint, int, int) \n" +
                            "X: " + x.ToString() + ", Y: " + y.ToString());
            }
        }

        public void CreateEntity(ushort dataIdKey, uint entityId, byte[] payload, ref int offset)
        {
            EntityData value = null;
            if (DataMappedToId.TryGetValue(dataIdKey, out value))
            {
                int x = BitConverter.ToInt32(payload, offset);
                offset += sizeof(int);
                int y = BitConverter.ToInt32(payload, offset);
                offset += sizeof(int);
                int actualPacketSize = value.PacketSize() - 8;

                byte[] arr = new byte[actualPacketSize];
                Array.Copy(payload, offset, arr, 0, actualPacketSize);
                offset += actualPacketSize;

                EntityInstance entityInstance = value.CreateInstance(entityId, x, y, DestroyEntity, arr);
                ActiveEntities.Add(entityInstance);
                InstanceMappedToId.Add(entityId, entityInstance);

                Debug.Log("[EntityCollection] - CreateEntity(ushort, int, int) \n" +
                            "X: " + x.ToString() + ", Y: " + y.ToString());
            }
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
                InstanceMappedToId.Remove(entity.Id);
                ActiveEntities.Remove(entity);
            }
            EntitiesToDestroy.Clear();
        }
    }
}
