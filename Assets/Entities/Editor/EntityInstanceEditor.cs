using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TheWorkforce.Entities
{
    [CustomEditor(typeof(EntityInstance))]
    public class EntityInstanceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    } 
}
