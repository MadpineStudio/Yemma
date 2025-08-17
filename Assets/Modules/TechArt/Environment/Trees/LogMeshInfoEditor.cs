using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(LogMeshInfo))]
public class LogMeshInfoEditor : Editor
{
    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Log Mesh Info"))
        {
            LogMeshInfo meshInfo = (LogMeshInfo)target;
            meshInfo.DebugMeshInfo();
        }
        
    }
}
