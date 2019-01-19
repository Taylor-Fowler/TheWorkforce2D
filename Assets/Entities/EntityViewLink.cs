using UnityEngine;

namespace TheWorkforce.Entities
{
    [CreateAssetMenu(fileName = "Entity View Link", menuName = "Entity Data/View Link")]
    public class EntityViewLink : ScriptableObject
    {
        public EntityView View;
    } 
}
