// Credit: https://gist.github.com/LotteMakesStuff/c0a3b404524be57574ffa5f8270268ea#file-readonlypropertydrawer-cs

// https://gist.githubusercontent.com/LotteMakesStuff/c0a3b404524be57574ffa5f8270268ea/raw/3ffec516d9e966f98ccb893e1e9040a06b41b793/ReadOnlyPropertyDrawer.cs
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}