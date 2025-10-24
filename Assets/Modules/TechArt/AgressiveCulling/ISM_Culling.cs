using UnityEngine;
using UnityEngine.Rendering;

public class ISM_Culling : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private Mesh instanceMesh;
    [SerializeField] private Material instanceMaterial;
    [SerializeField] private ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
    [SerializeField] private bool receiveShadows = true;
    [SerializeField] private int layer = 0;

    [Header("Instancing")]
    [SerializeField] private Transform[] bookTransforms;
    [SerializeField] private Bounds cullingBounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
    [SerializeField] private bool autoCalculateBounds = true;

    [Header("Culling")]
    [SerializeField] private ComputeShader cullingShader;
    [SerializeField] private bool enableFrustumCulling = true;
    [SerializeField] private bool enableDistanceCulling = true;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float instanceRadius = 0.5f;

    private ComputeBuffer argsBuffer;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer visibleIDBuffer;
    private ComputeBuffer instanceDataBuffer;
    
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private int cullingKernel;
    private Camera mainCamera;

    private struct InstanceData
    {
        public Matrix4x4 matrix;
        public Vector4 customData;
    }

    void Start()
    {
        mainCamera = Camera.main;
        InitializeBuffers();
    }

    void Update()
    {
        if (cullingShader != null && mainCamera != null)
        {
            PerformCulling();
        }
        RenderInstances();
    }

    void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void InitializeBuffers()
    {
        if (bookTransforms == null || bookTransforms.Length == 0)
        {
            Debug.LogError("ISM_Culling: Nenhum transform de livro atribuído!");
            return;
        }

        int instanceCount = bookTransforms.Length;

        // Args buffer para DrawMeshInstancedIndirect
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = instanceMesh != null ? instanceMesh.GetIndexCount(0) : 0;
        args[1] = 0; // Será preenchido pelo compute shader
        argsBuffer.SetData(args);

        // Buffer de dados das instâncias a partir dos transforms do Blender
        InstanceData[] instanceData = new InstanceData[instanceCount];
        Vector3 boundsMin = Vector3.one * float.MaxValue;
        Vector3 boundsMax = Vector3.one * float.MinValue;

        for (int i = 0; i < instanceCount; i++)
        {
            if (bookTransforms[i] == null) continue;

            Transform t = bookTransforms[i];
            instanceData[i].matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
            instanceData[i].customData = new Vector4(i, 0, 0, 0);

            // Calcular bounds automaticamente
            if (autoCalculateBounds)
            {
                boundsMin = Vector3.Min(boundsMin, t.position);
                boundsMax = Vector3.Max(boundsMax, t.position);
            }
        }

        // Atualizar bounds se auto-cálculo estiver ativo
        if (autoCalculateBounds)
        {
            Vector3 center = (boundsMin + boundsMax) / 2f;
            Vector3 size = (boundsMax - boundsMin) + Vector3.one * instanceRadius * 2f;
            cullingBounds = new Bounds(center, size);
        }

        instanceDataBuffer = new ComputeBuffer(instanceCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(InstanceData)));
        instanceDataBuffer.SetData(instanceData);

        // Buffer de IDs visíveis (append buffer)
        visibleIDBuffer = new ComputeBuffer(instanceCount, sizeof(uint), ComputeBufferType.Append);

        // Kernel do compute shader
        if (cullingShader != null)
        {
            cullingKernel = cullingShader.FindKernel("CSMain");
        }

        Debug.Log($"ISM_Culling: {instanceCount} livros inicializados. Bounds: {cullingBounds}");
    }

    private void PerformCulling()
    {
        if (bookTransforms == null || bookTransforms.Length == 0) return;

        visibleIDBuffer.SetCounterValue(0);

        cullingShader.SetBuffer(cullingKernel, "_InstanceDataBuffer", instanceDataBuffer);
        cullingShader.SetBuffer(cullingKernel, "_VisibleIDBuffer", visibleIDBuffer);
        cullingShader.SetBuffer(cullingKernel, "_ArgsBuffer", argsBuffer);
        
        cullingShader.SetInt("_InstanceCount", bookTransforms.Length);
        cullingShader.SetMatrix("_VPMatrix", GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false) * mainCamera.worldToCameraMatrix);
        cullingShader.SetVector("_CameraPosition", mainCamera.transform.position);
        cullingShader.SetFloat("_MaxDistance", maxDistance);
        cullingShader.SetFloat("_InstanceRadius", instanceRadius);
        cullingShader.SetInt("_EnableFrustumCulling", enableFrustumCulling ? 1 : 0);
        cullingShader.SetInt("_EnableDistanceCulling", enableDistanceCulling ? 1 : 0);

        int threadGroups = Mathf.CeilToInt(bookTransforms.Length / 64.0f);
        cullingShader.Dispatch(cullingKernel, threadGroups, 1, 1);

        // Copiar counter para args buffer
        ComputeBuffer.CopyCount(visibleIDBuffer, argsBuffer, sizeof(uint));
    }

    private void RenderInstances()
    {
        if (instanceMesh == null || instanceMaterial == null) return;

        instanceMaterial.SetBuffer("_VisibleIDBuffer", visibleIDBuffer);
        instanceMaterial.SetBuffer("_InstanceDataBuffer", instanceDataBuffer);

        Graphics.DrawMeshInstancedIndirect(
            instanceMesh,
            0,
            instanceMaterial,
            cullingBounds,
            argsBuffer,
            0,
            null,
            shadowCastingMode,
            receiveShadows,
            layer
        );
    }

    private void ReleaseBuffers()
    {
        argsBuffer?.Release();
        instanceDataBuffer?.Release();
        visibleIDBuffer?.Release();
    }
}
