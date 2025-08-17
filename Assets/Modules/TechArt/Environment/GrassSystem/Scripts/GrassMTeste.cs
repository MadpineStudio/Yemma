using System;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class GrassMTeste : MonoBehaviour
{
    // Configurações públicas
    public ComputeShader grassCompute;
    public Material grassMaterial;
    public Mesh grassSurfaceMesh;
    public int numQuads = 1000;
    public Texture2D grassTexture;
    [SerializeField] private float grassScale;
    
    // IDs de propriedades
    private static readonly int DrawTrianglesID = Shader.PropertyToID("_DrawTriangles");
    private static readonly int LocalToWorldID = Shader.PropertyToID("_LocalToWorld");
    private static readonly int NumQuadsID = Shader.PropertyToID("_NumQuads");
    private static readonly int MeshVertexBufferID = Shader.PropertyToID("_MeshVertexBuffer");
    private static readonly int MeshUVBufferID = Shader.PropertyToID("_MeshUVBuffer");
    private static readonly int MeshNormalBufferID = Shader.PropertyToID("_MeshNormalBuffer");
    private static readonly int MeshColorBufferID = Shader.PropertyToID("_MeshColorBuffer"); // Novo ID para vertex colors
    private static readonly int GrassRegionID = Shader.PropertyToID("_GrassRegion");
    private static readonly int GrassUVTexID = Shader.PropertyToID("_Grass_UVTex");
    
    // Buffers
    private ComputeBuffer _drawTriangleBuffer;
    private ComputeBuffer _meshVertexBuffer;
    private ComputeBuffer _meshUVBuffer;
    private ComputeBuffer _meshNormalBuffer;
    private ComputeBuffer _meshColorBuffer; // Novo buffer para vertex colors
    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _grassUVTex;

    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    private int _kernelHandle;

    void OnEnable()
    {
        Initialize();
    }

    void OnDisable()
    {
        ReleaseBuffers();
    }

    public void Initialize()
    {
        if (grassSurfaceMesh == null || grassTexture == null)
        {
            Debug.LogError("Missing required components");
            return;
        }

        ReleaseBuffers();
        
        // Configuração do buffer de argumentos para DrawProceduralIndirect
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        
        // Inicializa os buffers de dados
        InitializeGrassBuffers();
        SetComputeShaderData();
        DispatchComputeShader();
    }

    private void InitializeGrassBuffers()
    {
        grassCompute.SetFloat("Scale", grassScale);
        int totalTriangles = numQuads * 2; // 2 triângulos por quad
        int vertexStride = 3 * sizeof(float) * 8; // Stride de 36 bytes (3 vértices * 3 floats cada)
        _drawTriangleBuffer = new ComputeBuffer(totalTriangles, vertexStride, ComputeBufferType.Append);

        // Buffers para os dados da malha
        _meshVertexBuffer = new ComputeBuffer(grassSurfaceMesh.vertexCount, sizeof(float) * 3);
        _meshUVBuffer = new ComputeBuffer(grassSurfaceMesh.uv.Length, sizeof(float) * 2);
        _meshNormalBuffer = new ComputeBuffer(grassSurfaceMesh.normals.Length, sizeof(float) * 3);
        
        // Novo buffer para vertex colors
        Color[] meshColors = grassSurfaceMesh.colors;
        if (meshColors == null || meshColors.Length == 0)
        {
            // Se não houver vertex colors, cria um array padrão (branco)
            meshColors = new Color[grassSurfaceMesh.vertexCount];
            for (int i = 0; i < meshColors.Length; i++)
            {
                meshColors[i] = Color.white;
            }
        }
        _meshColorBuffer = new ComputeBuffer(meshColors.Length, sizeof(float) * 3); // RGB apenas
        
        // Converte Color[] para float3[] (ignorando alpha)
        Vector3[] colorData = new Vector3[meshColors.Length];
        for (int i = 0; i < meshColors.Length; i++)
        {
            colorData[i] = new Vector3(meshColors[i].r, meshColors[i].g, meshColors[i].b);
        }
        
        // Preenche os buffers com os dados da malha
        _meshVertexBuffer.SetData(grassSurfaceMesh.vertices);
        _meshUVBuffer.SetData(grassSurfaceMesh.uv);
        _meshNormalBuffer.SetData(grassSurfaceMesh.normals);
        _meshColorBuffer.SetData(colorData);
        
        // Cada quad precisa de 4 vértices (cada um com UVs diferentes)
        _grassUVTex = new ComputeBuffer(numQuads * 4, sizeof(float) * 2);
    }

    private void SetComputeShaderData()
    {
        _kernelHandle = grassCompute.FindKernel("CSGrassBlade");
        
        if (_kernelHandle < 0)
        {
            Debug.LogError("Kernel CSGrassBlade not found");
            return;
        }

        grassCompute.SetBuffer(_kernelHandle, DrawTrianglesID, _drawTriangleBuffer);
        grassCompute.SetInt(NumQuadsID, numQuads);
        grassCompute.SetMatrix(LocalToWorldID, transform.localToWorldMatrix);
        
        grassCompute.SetTexture(_kernelHandle, GrassRegionID, grassTexture);
        grassCompute.SetBuffer(_kernelHandle, MeshVertexBufferID, _meshVertexBuffer);
        grassCompute.SetBuffer(_kernelHandle, MeshUVBufferID, _meshUVBuffer);
        grassCompute.SetBuffer(_kernelHandle, MeshNormalBufferID, _meshNormalBuffer);
        grassCompute.SetBuffer(_kernelHandle, MeshColorBufferID, _meshColorBuffer); // Adiciona o buffer de cores
        grassCompute.SetBuffer(_kernelHandle, GrassUVTexID, _grassUVTex);
    }

    private void DispatchComputeShader()
    {
        if (_kernelHandle < 0) return;

        _drawTriangleBuffer.SetCounterValue(0);
        
        // Inicializa o buffer de UVs com valores padrão (opcional)
        float2[] initialUVs = new float2[numQuads * 4];
        _grassUVTex.SetData(initialUVs);
        
        int threadGroups = Mathf.CeilToInt((float)numQuads / 128);
        grassCompute.Dispatch(_kernelHandle, threadGroups, 1, 1);

        // Atualiza os argumentos de renderização
        _args[0] = 6 * (uint)numQuads; // Número de vértices
        _args[1] = 1; // Número de instâncias
        _argsBuffer.SetData(_args);
    }

    void Update()
    {
        if (grassMaterial == null || _drawTriangleBuffer == null)
            return;

        RenderGrass();
    }

    private void RenderGrass()
    {
        // Configura o material
        grassMaterial.SetMatrix(LocalToWorldID, transform.localToWorldMatrix);
        grassMaterial.SetBuffer(DrawTrianglesID, _drawTriangleBuffer);
        grassMaterial.SetBuffer(MeshVertexBufferID, _meshVertexBuffer);
        grassMaterial.SetBuffer(MeshUVBufferID, _meshUVBuffer);
        grassMaterial.SetBuffer(MeshNormalBufferID, _meshNormalBuffer);
        grassMaterial.SetBuffer(GrassUVTexID, _grassUVTex);

        // Renderização mais eficiente na Unity 5+
        Graphics.DrawProceduralIndirect(
            grassMaterial, 
            new Bounds(transform.position, Vector3.one * 100f), 
            MeshTopology.Triangles, 
            _argsBuffer, 
            0,
            null,
            null,
            UnityEngine.Rendering.ShadowCastingMode.Off,
            false,
            gameObject.layer
        );
    }

    private void ReleaseBuffers()
    {
        _drawTriangleBuffer?.Release();
        _meshVertexBuffer?.Release();
        _meshUVBuffer?.Release();
        _meshNormalBuffer?.Release();
        _meshColorBuffer?.Release(); // Libera o buffer de cores
        _grassUVTex?.Release();
        _argsBuffer?.Release();
        
        _drawTriangleBuffer = null;
        _meshVertexBuffer = null;
        _meshUVBuffer = null;
        _meshNormalBuffer = null;
        _meshColorBuffer = null;
        _argsBuffer = null;
        _grassUVTex = null;
    }

    public void Clear()
    {
        ReleaseBuffers();
    }
}