using UnityEngine;

public class ShadowTextureGenerator : MonoBehaviour
{
    public int textureSize = 256;
    public Transform headTransform; // Referência à cabeça do modelo

    private Texture2D shadowTexture;

    void Start()
    {
        if (headTransform == null)
        {
            Debug.LogError("Defina a referência da cabeça!");
            return;
        }

        shadowTexture = GenerateShadowTexture();
        GetComponent<Renderer>().material.SetTexture("_ShadowTex", shadowTexture);
    }

    Texture2D GenerateShadowTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

        Vector3 headForward = headTransform.forward;
        Vector3 headRight = headTransform.right;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float u = (x / (float)textureSize) * 2.0f - 1.0f; // Normalizado de -1 a 1
                float v = (y / (float)textureSize) * 2.0f - 1.0f; // Normalizado de -1 a 1

                Vector3 direction = headForward * v + headRight * u;
                direction.Normalize();

                float dotForward = Vector3.Dot(direction, Vector3.forward);
                float dotRight = Vector3.Dot(direction, Vector3.right);

                float red = Mathf.Clamp01(dotForward * 0.5f + 0.5f); // Luz vinda da frente
                float green = Mathf.Clamp01(dotRight * 0.5f + 0.5f); // Luz vinda da direita

                Color color = new Color(red, green, 0.0f);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }
}