using UnityEngine;
using UnityEditor;

namespace TheWorkforce.Items
{
    //[CustomEditor(typeof(ItemController))]
    //public class ItemControllerEditor : Editor
    //{
    //    public override void OnInspectorGUI()
    //    {
    //        ItemController itemController = target as ItemController;

    //        if(itemController.Item != null)
    //        {

    //            GUIStyle width = new GUIStyle(GUI.skin.label)
    //            {
    //                fixedWidth = 100
    //            };

    //            GUIStyle secondWidth = new GUIStyle(GUI.skin.label)
    //            {
    //                fixedWidth = 200
    //            };

    //            ItemData item = itemController.Item;
    //            EditorGUILayout.BeginHorizontal();
    //            {
    //                EditorGUILayout.BeginVertical(width);
    //                {
    //                    EditorGUILayout.LabelField("Id");
    //                    EditorGUILayout.LabelField("Name");
    //                    EditorGUILayout.LabelField("Description");
    //                }
    //                EditorGUILayout.EndVertical();

    //                EditorGUILayout.BeginVertical(secondWidth);
    //                {
    //                    EditorGUILayout.LabelField(item.Id.ToString());
    //                    EditorGUILayout.LabelField(item.Name);
    //                    EditorGUILayout.LabelField(item.Description);
    //                }
    //                EditorGUILayout.EndVertical();

    //                EditorGUILayout.BeginVertical();
    //                {   
    //                    GUILayout.Box(item.Sprite.texture);
    //                }
    //                EditorGUILayout.EndVertical();
    //            }
    //            EditorGUILayout.EndHorizontal();
    //        }
    //    }
    //}
}
