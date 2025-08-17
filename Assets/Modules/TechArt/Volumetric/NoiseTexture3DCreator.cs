// Editor script para criar textura 3D de ruído
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class NoiseTexture3DCreator : EditorWindow
{
    [MenuItem("Assets/Create/3D Noise Texture")]
    public static void Create()
    {
        int size = 8;
        Texture3D texture = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
        
        Color[] colors = new Color[size * size * size];
        
        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float value = Random.value;
                    colors[x + y * size + z * size * size] = new Color(value, value, value, 1);
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        AssetDatabase.CreateAsset(texture, "Assets/NoiseTexture3D.asset");
    }
}
#endif