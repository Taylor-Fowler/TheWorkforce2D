using UnityEngine;
using UnityEditor;

namespace TheWorkforce.Entities
{
    [CustomEditor(typeof(EntityCollection))]
    public class EntityCollectionEditor : Editor
    {
        private int _searchEntityId = 0;
        private EntityInstance _searchedEntity = null;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EntityCollection ec = target as EntityCollection;

            int searchField = _searchEntityId;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search");
            searchField = EditorGUILayout.IntField(searchField);
            EditorGUILayout.EndHorizontal();


            if(searchField != 0 && searchField != _searchEntityId)
            {
                _searchedEntity = ec.GetEntity((uint)_searchEntityId);

                _searchEntityId = searchField;
            }
            if(_searchedEntity != null)
            {
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("X: " + _searchedEntity.X.ToString() +
                                            " Y: " + _searchedEntity.Y.ToString());
                GUILayout.EndVertical();
            }

        }
    }

}