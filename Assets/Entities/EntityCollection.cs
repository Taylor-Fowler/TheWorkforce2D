using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Entities
{
    using Game_State; using Interfaces;

    [CreateAssetMenu(fileName = "Entity Collection", menuName = "Entity Data/Collection")]
    public class EntityCollection : ScriptableObject, IManager
    {
        #region Singleton
        public static EntityCollection Instance { get; private set; }
        #endregion

        public GameManager GameManager { get; private set; }
        public uint EntityIdCounter { get; private set; }
        public List<EntityData> Collection;
        public List<EntityInstance> ActiveEntities;
        public Dictionary<ushort, EntityData> DataMappedToId;
        public Dictionary<uint, EntityInstance> InstanceMappedToId;
        public List<EntityInstance> EntitiesToUnload;

        private ushort _dataIdCounter;

        public void Startup(GameManager gameManager)
        {
            Instance = this;
            GameManager = gameManager;
            GameManager.OnApplicationStateChange += GameManager_OnApplicationStateChange;

            EntityIdCounter = _dataIdCounter = 0;
            DataMappedToId = new Dictionary<ushort, EntityData>();

            foreach (var value in Collection)
            {
                value.Initialise(++_dataIdCounter);
                DataMappedToId.Add(_dataIdCounter, value);
            }

            GameTime.SubscribeToPostUpdate(UnloadEntities);
        }

        private void GameManager_OnApplicationStateChange(ApplicationStateChangeArgs applicationStateChange)
        {
            switch(applicationStateChange.Current)
            {
                case (EApplicationState.Connecting):
                    {
                        Initialise();
                        break;
                    }
                case (EApplicationState.ReturningToMenu):
                    {
                        ActiveEntities.Clear();
                        InstanceMappedToId.Clear();
                        EntitiesToUnload.Clear();
                        EntityIdCounter = 0;
                        break;
                    }
                default:
                    break;
            }
        }

        private void Initialise()
        {
            ActiveEntities = new List<EntityInstance>();
            InstanceMappedToId = new Dictionary<uint, EntityInstance>();
            EntitiesToUnload = new List<EntityInstance>();
        }

        public EntityInstance GetEntity(uint entityInstanceId)
        {
            EntityInstance value;
            InstanceMappedToId.TryGetValue(entityInstanceId, out value);
            return value;
        }

        public void UpdateEntityCounter(uint counter) => EntityIdCounter = counter;

        // Server side
        public uint CreateEntity(ushort dataIdKey, Tile tile, Vector2Int worldPosition)
        {
            EntityData value;
            if(DataMappedToId.TryGetValue(dataIdKey, out value))
            {
                EntityIdCounter++;

                EntityInstance instance = value.CreateInstance(EntityIdCounter, worldPosition.x, worldPosition.y, UnloadEntity);
                AddInstance(instance, tile);

                return EntityIdCounter;
            }
            return 0;
        }

        // Server and client side
        public void LoadEntity(ushort dataIdKey, uint entityId, Tile tile, Vector2Int worldPosition, byte[] saveData, int offset)
        {
            EntityData value = null;
            if (DataMappedToId.TryGetValue(dataIdKey, out value))
            {
                byte[] entityData = new byte[saveData.Length - offset];
                Array.Copy(saveData, offset, entityData, 0, entityData.Length);

                EntityInstance entityInstance = value.CreateInstance(entityId, worldPosition.x, worldPosition.y, UnloadEntity, entityData);
                AddInstance(entityInstance, tile);
            }
        }

        // Client side - from network message
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

                Tile tile = GameManager.WorldController.World[new Vector2Int(x, y)];
                EntityInstance entityInstance = value.CreateInstance(entityId, x, y, UnloadEntity, arr);

                AddInstance(entityInstance, tile);
            }
        }

        private void AddInstance(EntityInstance entityInstance, Tile tile)
        {
            ActiveEntities.Add(entityInstance);
            InstanceMappedToId.Add(entityInstance.Id, entityInstance);
 
            tile.OnUnload += entityInstance.Unload;
            entityInstance.OnEntityDestroy += tile.RemoveEntity;
        }

        private void UnloadEntity(uint entityInstanceId)
        {
            //Debug.Log($"[EntityCollection] - UnloadEntity(uint) \nEntity ID: {entityInstanceId.ToString()}");

            var instance = GetEntity(entityInstanceId);
            if(instance != null)
            {
                EntitiesToUnload.Add(instance);
            }
        }

        private void UnloadEntities()
        {
            foreach (var entity in EntitiesToUnload)
            {
                InstanceMappedToId.Remove(entity.Id);
                ActiveEntities.Remove(entity);
            }
            EntitiesToUnload.Clear();
        }
    }
}
