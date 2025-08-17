using UnityEngine;
using UnityEngine.Serialization;

public class GrassTekDisplacement : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera maskCamera;
    public RenderTexture maskRenderTexture;
    public Transform playerTransformRef;
    private Vector3 _playerVelocity;
    private Vector3 _lastPlayerPosition;
    
    [Header("Interaction Settings")]
    public ComputeShader maskComputeShader;
    [Range(0.01f, 1f)] public float decayRate = 0.1f;
    [Range(.1f, 1f)] public float fadeExponent = 1.5f; // Controle da curvatura do fade
    public float updateInterval = 0.1f;

    [Header("Debug")]
    [FormerlySerializedAs("renderMaterial")] 
    public Material debugMaterial;
    public Material grassMaterial;

    private RenderTexture _interactionBuffer;
    private int _updateKernel;
    private float _lastUpdateTime;
    
    private static readonly int DecayRateID = Shader.PropertyToID("_DecayRate");
    private static readonly int TimeDeltaID = Shader.PropertyToID("_TimeDelta");
    private static readonly int FadeExponentID = Shader.PropertyToID("_FadeExponent");
    private static readonly int GrassMaskBufferID = Shader.PropertyToID("_GrassMaskBuffer");
    private static readonly int InputMaskID = Shader.PropertyToID("_InputMask");
    private static readonly int PlayerTransformID = Shader.PropertyToID("_PlayerTransform");
    private static readonly int PlayerVelocityID = Shader.PropertyToID("_PlayerVelocity");
    void Start()
    {
        ValidateReferences();
        _interactionBuffer = CreateGPUCompatibleRT(maskRenderTexture.width, maskRenderTexture.height, RenderTextureFormat.ARGBFloat);
        _updateKernel = maskComputeShader.FindKernel("UpdateMask");
        
        if (maskCamera.targetTexture == null)
            maskCamera.targetTexture = maskRenderTexture;
    }

    void Update()
    {
        // delta velocity
        var position = playerTransformRef.position;
        _playerVelocity = (position - _lastPlayerPosition) / Time.deltaTime;
        _lastPlayerPosition = position;
        
        if (Time.time - _lastUpdateTime >= updateInterval)
        {
            UpdateInteractionBuffer();
            _lastUpdateTime = Time.time;
        }

        if (debugMaterial != null)
        {
            debugMaterial.SetTexture(InputMaskID, _interactionBuffer);
            grassMaterial.SetTexture(InputMaskID, _interactionBuffer);
        }
    }

    void UpdateInteractionBuffer()
    {
        maskCamera.Render();
        
        // Configurar parâmetros do fade
        maskComputeShader.SetFloat(DecayRateID, decayRate);
        maskComputeShader.SetFloat(TimeDeltaID, updateInterval);
        maskComputeShader.SetFloat(FadeExponentID, fadeExponent);
        maskComputeShader.SetVector(PlayerTransformID, playerTransformRef.position);
        maskComputeShader.SetVector(PlayerVelocityID, _lastPlayerPosition);
        
        // Passar texturas
        maskComputeShader.SetTexture(_updateKernel, GrassMaskBufferID, _interactionBuffer);
        maskComputeShader.SetTexture(_updateKernel, InputMaskID, maskRenderTexture);
        
        // Executar
        int groupsX = Mathf.CeilToInt(maskRenderTexture.width / 8f);
        int groupsY = Mathf.CeilToInt(maskRenderTexture.height / 8f);
        maskComputeShader.Dispatch(_updateKernel, groupsX, groupsY, 1);
    }

    RenderTexture CreateGPUCompatibleRT(int width, int height, RenderTextureFormat format)
    {
        var rt = new RenderTexture(width, height, 0, format)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            autoGenerateMips = false
        };
        rt.Create();
        return rt;
    }

    void ValidateReferences()
    {
        if (maskCamera == null || maskRenderTexture == null || maskComputeShader == null)
        {
            Debug.LogError("Verifique se a Mask Camera, Render Texture e Compute Shader estão atribuídos.", this);
            enabled = false;
        }
    }

    void OnDestroy()
    {
        if (_interactionBuffer != null && _interactionBuffer.IsCreated())
            _interactionBuffer.Release();
    }
}