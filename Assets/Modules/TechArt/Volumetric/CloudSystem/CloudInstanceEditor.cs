using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CloudInstance))]
public class CloudInstanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // if (GUILayout.Button("Add Cloud"))
        // {
        //     CloudInstance cloud = (CloudInstance)target;
        //     cloud.Add();
        // }
    }
}
