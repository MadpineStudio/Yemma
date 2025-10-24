using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper para importar transforms dos livros do Blender para o ISM_Culling
/// </summary>
public class ISM_BookImporter : MonoBehaviour
{
    [Header("Import Settings")]
    [SerializeField] private Transform blenderRootObject;
    [SerializeField] private string bookNameFilter = "Book";
    [SerializeField] private ISM_Culling targetCullingSystem;

    [Header("Preview")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoSize = 0.1f;

    private Transform[] detectedBooks;

    public void ScanBooks()
    {
        if (blenderRootObject == null)
        {
            Debug.LogError("ISM_BookImporter: Root object não atribuído!");
            return;
        }

        // Encontrar todos os transforms que correspondem ao filtro
        Transform[] allChildren = blenderRootObject.GetComponentsInChildren<Transform>();
        System.Collections.Generic.List<Transform> books = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.name.Contains(bookNameFilter))
            {
                books.Add(child);
            }
        }

        detectedBooks = books.ToArray();
        Debug.Log($"ISM_BookImporter: {detectedBooks.Length} livros detectados com filtro '{bookNameFilter}'");
    }

    public void ApplyToISM()
    {
        if (targetCullingSystem == null)
        {
            Debug.LogError("ISM_BookImporter: Sistema de culling não atribuído!");
            return;
        }

        if (detectedBooks == null || detectedBooks.Length == 0)
        {
            Debug.LogWarning("ISM_BookImporter: Nenhum livro detectado. Execute ScanBooks() primeiro.");
            return;
        }

#if UNITY_EDITOR
        SerializedObject so = new SerializedObject(targetCullingSystem);
        SerializedProperty prop = so.FindProperty("bookTransforms");
        
        prop.ClearArray();
        prop.arraySize = detectedBooks.Length;
        
        for (int i = 0; i < detectedBooks.Length; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = detectedBooks[i];
        }
        
        so.ApplyModifiedProperties();
        Debug.Log($"ISM_BookImporter: {detectedBooks.Length} livros aplicados ao sistema ISM_Culling");
#endif
    }

    public void DisableOriginalMeshes()
    {
        if (detectedBooks == null || detectedBooks.Length == 0) return;

        foreach (Transform book in detectedBooks)
        {
            MeshRenderer renderer = book.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        Debug.Log($"ISM_BookImporter: MeshRenderers desabilitados em {detectedBooks.Length} livros");
    }

    public void EnableOriginalMeshes()
    {
        if (detectedBooks == null || detectedBooks.Length == 0) return;

        foreach (Transform book in detectedBooks)
        {
            MeshRenderer renderer = book.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        Debug.Log($"ISM_BookImporter: MeshRenderers habilitados em {detectedBooks.Length} livros");
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || detectedBooks == null) return;

        Gizmos.color = gizmoColor;
        foreach (Transform book in detectedBooks)
        {
            if (book != null)
            {
                Gizmos.DrawWireSphere(book.position, gizmoSize);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ISM_BookImporter))]
public class ISM_BookImporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ISM_BookImporter importer = (ISM_BookImporter)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Workflow", EditorStyles.boldLabel);

        if (GUILayout.Button("1. Scan Books", GUILayout.Height(30)))
        {
            importer.ScanBooks();
        }

        if (GUILayout.Button("2. Apply to ISM System", GUILayout.Height(30)))
        {
            importer.ApplyToISM();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mesh Management", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Disable Original Meshes"))
        {
            importer.DisableOriginalMeshes();
        }
        if (GUILayout.Button("Enable Original Meshes"))
        {
            importer.EnableOriginalMeshes();
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
