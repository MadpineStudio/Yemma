using UnityEngine;

public class Cloth : MonoBehaviour
{
    [Header("Settings")]
    public float gravityStrength = 9.8f;
    public bool visualizeSimulation = true;

    [Header("References")]
    public ComputeShader computeShader;
    public Mesh originalMesh;

    private ComputeBuffer _vertexBuffer;
    private int _kernelHandle;
    private Mesh _displayMesh;
    private VertexData[] _vertexData;

    private void Start()
    {
        InitializeSimulation();
    }

    private void InitializeSimulation()
    {
        // Criamos uma cópia apenas para visualização (opcional)
        if (visualizeSimulation)
        {
            _displayMesh = new Mesh();
            _displayMesh.name = "SimulationMesh";
            GetComponent<MeshFilter>().mesh = _displayMesh;
        }

        // Preparamos os dados dos vértices
        _vertexData = PrepareVertexData(originalMesh);

        // Criamos o buffer
        _vertexBuffer = new ComputeBuffer(_vertexData.Length, VertexData.Size());

        // Configuramos o compute shader
        _kernelHandle = computeShader.FindKernel("ApplyGravity");
        computeShader.SetBuffer(_kernelHandle, "vertices", _vertexBuffer);
    }

    private VertexData[] PrepareVertexData(Mesh mesh)
    {
        Vector3[] positions = mesh.vertices;
        Vector3[] normals = mesh.normals.Length == positions.Length ? mesh.normals : null;
        Vector2[] uvs = mesh.uv.Length == positions.Length ? mesh.uv : null;

        VertexData[] data = new VertexData[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            data[i] = new VertexData
            {
                position = positions[i],
                normal = normals != null ? normals[i] : Vector3.up,
                uv = uvs != null ? uvs[i] : Vector2.zero
            };
        }

        return data;
    }

    private void Update()
    {
        // Atualizamos os parâmetros
        computeShader.SetFloat("gravityStrength", gravityStrength);
        computeShader.SetFloat("deltaTime", Time.deltaTime);

        // Enviamos os dados para a GPU
        _vertexBuffer.SetData(_vertexData);

        // Executamos o compute shader
        int threadGroups = Mathf.CeilToInt(_vertexData.Length / 64f);
        computeShader.Dispatch(_kernelHandle, threadGroups, 1, 1);

        // Recuperamos os dados (opcional)
        if (visualizeSimulation)
        {
            UpdateVisualization();
        }
    }

    private void UpdateVisualization()
    {
        // Pegamos os dados do buffer
        _vertexBuffer.GetData(_vertexData);

        // Atualizamos a mesh de visualização
        Vector3[] positions = new Vector3[_vertexData.Length];
        for (int i = 0; i < _vertexData.Length; i++)
        {
            positions[i] = _vertexData[i].position;
        }

        _displayMesh.Clear();
        _displayMesh.vertices = positions;
        _displayMesh.normals = originalMesh.normals;
        _displayMesh.uv = originalMesh.uv;
        _displayMesh.triangles = originalMesh.triangles;
    }

    private void OnDestroy()
    {
        _vertexBuffer?.Release();
        if (_displayMesh != null && Application.isPlaying)
        {
            Destroy(_displayMesh);
        }
    }

    private struct VertexData
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public static int Size()
        {
            return sizeof(float) * 8; // 3(pos) + 3(normal) + 2(uv)
        }
    }
}