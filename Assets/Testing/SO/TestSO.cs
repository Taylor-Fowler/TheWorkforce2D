using TheWorkforce.Entities;
using UnityEngine;

namespace TheWorkforce.Testing.SO
{
    public class TestSO : MonoBehaviour
    {
        public EntityDataCollection ObjectDataCollection;

        private void Start()
        {
            ObjectDataCollection.Initialise();
            uint entityId = 0;

            for(int x = 0; x < 10; x++)
            {
                for(int y = 0; y < 10; y++)
                {
                    var obj = ObjectDataCollection.Collection[Random.Range(0, 2)];
                    var go = obj.SpawnObject(obj.CreateInstance(++entityId));
                    go.transform.position = new Vector3(x, y, 0);
                }
            }
        }
    }

}