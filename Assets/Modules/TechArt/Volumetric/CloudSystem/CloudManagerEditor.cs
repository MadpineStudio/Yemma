using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CloudManager))]
public class CloudManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CloudManager cloudManager = (CloudManager)target;

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Buffer Controls", EditorStyles.boldLabel);
        if (GUILayout.Button("Allocate Buffer"))
        {
            cloudManager.SetupBuffers();
        }

        if (GUILayout.Button("Release Buffer"))
        {
            cloudManager.ClearBuffers();
        }
    }
}