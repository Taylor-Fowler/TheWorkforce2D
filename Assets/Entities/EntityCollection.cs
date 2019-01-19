using System.Collections.Generic;
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

        private ushort _dataIdCounter = 0;
        private uint _entityIdCounter = 0;

        public void Initialise()
        {
            _instance = this;
            DataMappedToId = new Dictionary<ushort, EntityData>();
            InstanceMappedToId = new Dictionary<uint, EntityInstance>();

            foreach(var value in Collection)
            {
                value.Initialise(++_dataIdCounter);
                DataMappedToId.Add(_dataIdCounter, value);
            }
        }

        public uint CreateEntity(ushort dataIdKey)
        {
            Debug.Log("[EntityCollection] - CreateEntity(ushort)");

            EntityData value = null;

            if(DataMappedToId.TryGetValue(dataIdKey, out value))
            {
                InstanceMappedToId.Add(++_entityIdCounter, value.CreateInstance(_entityIdCounter));
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
    }
}
