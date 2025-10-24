using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColliderManager))]
public class ColliderManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ColliderManager manager = (ColliderManager)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Adicionar Mesh Colliders nos Filhos", GUILayout.Height(40)))
        {
            manager.AddMeshCollidersToChildren();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Tornar Filhos Estáticos", GUILayout.Height(40)))
        {
            manager.SetChildrenStatic();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Aplicar Material nos Filhos", GUILayout.Height(40)))
        {
            manager.SetMaterialToChildren();
        }
    }
}
