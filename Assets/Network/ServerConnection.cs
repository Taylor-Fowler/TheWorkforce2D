using UnityEditor;
using UnityEngine;

public class ServerConnection : MonoBehaviour
{
    private void Awake()
    {
        Connect();
    }

    public void Connect()
    {
    }

    public void Disconnect()
    {
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ServerConnection))]
public class ServerConnectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ServerConnection connection = (ServerConnection) target;
        GUIStyle _connectStyle = new GUIStyle(GUI.skin.button);
        GUIStyle _disconnectStyle = new GUIStyle(GUI.skin.button);

        _connectStyle.normal.textColor = Color.green;
        _disconnectStyle.normal.textColor = Color.red;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Connect", _connectStyle, GUILayout.MaxWidth(100))) connection.Connect();
        if (GUILayout.Button("Disconnect", _disconnectStyle, GUILayout.MaxWidth(100))) connection.Disconnect();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
#endif