using UnityEngine;

public class LogMeshInfo : MonoBehaviour
{
    [SerializeField] private Mesh mesh;

    public void DebugMeshInfo()
    {
        int id = 0;
        Debug.Log("LINEAR");
        foreach (var item in mesh.colors)
        {
            Color linearColor = item.linear;

            float r = linearColor.r * 2 - 1;
            float g = linearColor.g * 2 - 1;
            float b = linearColor.b * 2 - 1;
            Vector3 col = new Vector3(r, g, b);
            Debug.Log($"{id} -> Color: {col}");
            id++;
        }

        // Debug.Log("VERTEX ADJUSTED");
        // id = 0;
        // foreach (var item in mesh.colors)
        // {
        //     Color linearColor = item.linear;
        //     Vector4 res = TruncToVector3Decimals(linearColor);
        //     Debug.Log($"{id} -> Color: {res}");
        //     id++;
        // }
    }
    public static Vector4 TruncToVector3Decimals(Color linearColor)
    {
        // Passo 1: Trunca cada componente para 3 casas decimais (sem arredondar)
        float r = Mathf.Floor(linearColor.r * 1000f) / 1000f;
        float g = Mathf.Floor(linearColor.g * 1000f) / 1000f;
        float b = Mathf.Floor(linearColor.b * 1000f) / 1000f;
        float a = linearColor.a; // Alpha permanece inalterado

        // Passo 2: Retorna como Vector4 (x=R, y=G, z=B, w=A)
        return new Vector4(r, g, b, a) * 2 - Vector4.one;
    }
}
