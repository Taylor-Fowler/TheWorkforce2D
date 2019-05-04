using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MyButton))]
public class MyButtonEditor : Editor
{
    private bool _showEvents;

    private SerializedProperty _interactable;
    private SerializedProperty _useExitAsUp;
    private SerializedProperty _onEnter;
    private SerializedProperty _onClick;
    private SerializedProperty _onExit;
    private SerializedProperty _onUp;

    private void OnEnable()
    {
        _interactable = serializedObject.FindProperty("Interactable");
        _useExitAsUp = serializedObject.FindProperty("UseExitAsUp");
        _onEnter = serializedObject.FindProperty("OnMouseEnter");
        _onClick = serializedObject.FindProperty("OnClick");
        _onExit = serializedObject.FindProperty("OnMouseExit");
        _onUp = serializedObject.FindProperty("OnUp");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(_interactable);
        EditorGUILayout.PropertyField(_useExitAsUp);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        _showEvents = EditorGUILayout.Foldout(_showEvents, "Edit Events");

        if(_showEvents)
        {
            EditorGUILayout.PropertyField(_onEnter, true);
            EditorGUILayout.PropertyField(_onClick, true);
            EditorGUILayout.PropertyField(_onExit, true);
            EditorGUILayout.PropertyField(_onUp, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
