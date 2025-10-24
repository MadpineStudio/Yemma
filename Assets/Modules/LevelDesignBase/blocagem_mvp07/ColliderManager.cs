using UnityEngine;

public class ColliderManager : MonoBehaviour
{
    public Material material;

    public void AddMeshCollidersToChildren()
    {
        // Navega por todos os filhos
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            // Pula o próprio objeto
            if (child == transform) continue;

            // Verifica se tem MeshFilter e se não tem MeshCollider
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            MeshCollider meshCollider = child.GetComponent<MeshCollider>();

            if (meshFilter != null && meshCollider == null)
            {
                // Adiciona MeshCollider
                child.gameObject.AddComponent<MeshCollider>();
                Debug.Log($"MeshCollider adicionado em: {child.name}");
            }
        }

        Debug.Log("Processo concluído!");
    }

    public void SetChildrenStatic()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child == transform) continue;

            child.gameObject.isStatic = true;
            Debug.Log($"GameObject setado como estático: {child.name}");
        }

        Debug.Log("Todos os filhos estão estáticos!");
    }

    public void SetMaterialToChildren()
    {
        if (material == null)
        {
            Debug.LogWarning("Nenhum material foi atribuído!");
            return;
        }

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child == transform) continue;

            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                Debug.Log($"Material aplicado em: {child.name}");
            }
        }

        Debug.Log("Material aplicado em todos os filhos!");
    }
}
