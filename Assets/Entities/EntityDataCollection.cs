using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Entity Data Collection", menuName = "Entity Data/Collection")]
    public class EntityDataCollection : ScriptableObject
    {
        public List<EntityData> Collection;
        public Dictionary<ushort, EntityData> DataMappedToId;
        private ushort _idCounter = 0;

        public void Initialise()
        {
            DataMappedToId = new Dictionary<ushort, EntityData>();
            foreach(var value in Collection)
            {
                value.Initialise(++_idCounter);
                DataMappedToId.Add(_idCounter, value);
            }
        }
    }
}
