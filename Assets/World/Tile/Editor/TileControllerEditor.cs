using UnityEditor;

namespace TheWorkforce.World
{
    [CustomEditor(typeof(TileController))]
    public class TileControllerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            TileController tileController = target as TileController;

            //if(tileController.ItemController)
            //{
            //    GUILayout.BeginVertical("Box");
            //    Editor editor = CreateEditor(tileController.ItemController);
            //    editor.OnInspectorGUI();
            //    GUILayout.EndVertical();
            //}

            serializedObject.ApplyModifiedProperties();
        }
    }
}
