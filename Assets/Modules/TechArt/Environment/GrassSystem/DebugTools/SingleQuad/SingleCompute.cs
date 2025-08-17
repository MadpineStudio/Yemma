using UnityEngine;

[ExecuteInEditMode]
public class SingleCompute : MonoBehaviour
{
    // Configurações públicas
    public ComputeShader grassCompute; // Compute Shader para gerar a grama
    public Material grassMaterial;    // Material para renderizar a grama
    public Mesh grassSurfaceMesh;     // Malha da superfície onde a grama será gerada
    public int numQuads;              // Número de quads (retângulos) de grama por vértice

    // IDs das propriedades do shader
    private static readonly int DrawTrianglesID = Shader.PropertyToID("_DrawTriangles");
    private static readonly int LocalToWorldID = Shader.PropertyToID("_LocalToWorld");
    private static readonly int NumQuadsID = Shader.PropertyToID("_NumQuads");
    private static readonly int TimeID = Shader.PropertyToID("_Time");
    private static readonly int MeshVertexBufferID = Shader.PropertyToID("_MeshVertexBuffer");
    private static readonly int MeshVertexNormalsBufferID = Shader.PropertyToID("_MeshVertexNormalsBuffer");

    // Buffers de dados
    private ComputeBuffer _drawTriangleBuffer; 
    private ComputeBuffer _meshVertexBuffer; 
    private ComputeBuffer _meshVertexNormalBuffer; 
    
    private int _kernelQuantGrassHandle;
    private int _kernelHandle;
    
    [SerializeField] private VertexPaintData _vertexPaintData;
    private static readonly int VertexPaintDataID = Shader.PropertyToID("_VertexPaintData");
    private ComputeBuffer _vertexPaintBuffer;
    
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void Calculate()
    {
        grassMaterial.SetPass(0);
        
        InitializeBuffers(); 
        PrepareMeshVertexData();

        SetComputeShaderData(); 
        DispatchComputeShader(); 
    }

    private void InitializeBuffers()
    {
        if (grassSurfaceMesh == null )
        {
            Debug.LogError("Dados de entrada não configurados corretamente.");
            return;
        }

        // Libera os buffers antigos antes de criar novos
        DiscardBuffers();

        // Buffer para armazenar os triângulos gerados
        int totalQuads = numQuads ; 
        // 23 floats
        int triangleStride = 26 * sizeof(float);
        _drawTriangleBuffer = new ComputeBuffer(totalQuads, triangleStride, ComputeBufferType.Append);

        _meshVertexBuffer = new ComputeBuffer(grassSurfaceMesh.vertexCount, sizeof(float) * 3);
        _meshVertexNormalBuffer = new ComputeBuffer(grassSurfaceMesh.normals.Length, sizeof(float) * 3);
        
        //vertex paint initialization
        _vertexPaintBuffer = new ComputeBuffer(_vertexPaintData.vertexColors.Length, sizeof(float) * 4);
        
        grassMaterial.SetMatrix(LocalToWorldID, transform.localToWorldMatrix);
        grassMaterial.SetBuffer(DrawTrianglesID, _drawTriangleBuffer);
        grassMaterial.SetBuffer(MeshVertexBufferID, _meshVertexBuffer);
    }

    private void SetComputeShaderData()
    {
        _kernelHandle = grassCompute.FindKernel("CSGrassBlade");
        
        if (_kernelHandle < 0)
        {
            Debug.LogError("Kernel CSGrassBlade não encontrado no Compute Shader.");
            return;
        }

        grassCompute.SetBuffer(_kernelHandle, DrawTrianglesID, _drawTriangleBuffer);
        
        grassCompute.SetInt(NumQuadsID, numQuads);
        grassCompute.SetFloat(TimeID, Time.deltaTime);
        grassCompute.SetMatrix(LocalToWorldID, transform.localToWorldMatrix);
    }

    private void PrepareMeshVertexData()
    {
        // Passa os vértices da malha
        Vector3[] vertices = grassSurfaceMesh.vertices;
        _meshVertexBuffer.SetData(vertices);
        grassCompute.SetBuffer(_kernelHandle, MeshVertexBufferID, _meshVertexBuffer);

        Vector3[] surfaceNormals = grassSurfaceMesh.normals;
        _meshVertexNormalBuffer.SetData(surfaceNormals);
        grassCompute.SetBuffer(_kernelHandle, MeshVertexNormalsBufferID, _meshVertexNormalBuffer);
        
        // vertex paint data to buffer
        _vertexPaintBuffer.SetData(_vertexPaintData.vertexColors);
        grassCompute.SetBuffer(_kernelHandle, VertexPaintDataID, _vertexPaintBuffer );
        
    }
    
    public void DispatchComputeShader()
    {
        // Verifica se o Compute Shader está configurado corretamente
        if (_kernelHandle < 0 || _kernelQuantGrassHandle < 0)
        {
            Debug.LogError("Kernel não configurado corretamente.");
            return;
        }

        // Limpa o buffer de triângulos antes de executar o Compute Shader
        _drawTriangleBuffer.SetCounterValue(0);
        _meshVertexBuffer.SetCounterValue(0);
        _meshVertexNormalBuffer.SetCounterValue(0);
        _vertexPaintBuffer.SetCounterValue(0);
        
        int threadGroups = Mathf.CeilToInt((float)numQuads / 128);
        grassCompute.Dispatch(_kernelHandle, threadGroups, 1, 1);

        Debug.Log("Compute Shader executado com sucesso.");
    }

    public void Paint()
    {
        Clear();
        Calculate();
    }
    // Renderiza a grama após a cena ser renderizada.
    private void OnRenderObject()
    {
        if (grassMaterial == null)
        {
            Debug.LogError("Material não configurado corretamente.");
            return;
        }

        grassMaterial.SetPass(0);

        Graphics.DrawProceduralNow(MeshTopology.Quads, numQuads * 4);
    }

    public void Clear()
    {
        DiscardBuffers();
    }
    // Libera os buffers para evitar vazamentos de memória.
    private void DiscardBuffers()
    {
        _meshVertexNormalBuffer?.Release();
        _meshVertexNormalBuffer = null;

        _drawTriangleBuffer?.Release();
        _drawTriangleBuffer = null;

        _meshVertexBuffer?.Release();
        _meshVertexBuffer = null;
        
        _vertexPaintBuffer?.Release();
        _vertexPaintBuffer = null;
    }
}

