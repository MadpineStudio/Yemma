using UnityEditor; using UnityEngine;

[CustomEditor(typeof(GrassMTeste))]
public class GrassManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 
        GrassMTeste grassTarget = (GrassMTeste)target; 
        if (GUILayout.Button("CalculateGrass")) 
            grassTarget.Initialize(); 
        if (GUILayout.Button("Clear Buffers")) 
            grassTarget.Clear();
    }
}
