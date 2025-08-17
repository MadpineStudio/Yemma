using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SingleCompute))]
public class SingleQuadComputeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 
        SingleCompute grassTarget = (SingleCompute)target; 
        if (GUILayout.Button("Dispatch SingleQuad")) 
            grassTarget.Calculate(); 
        if (GUILayout.Button("Clear Buffers")) 
            grassTarget.Clear();
    }
}
