using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class VolumetricFog : MonoBehaviour
{
    [Header("Volume Settings")]
    public Vector3 volumeSize = new Vector3(50, 20, 50);
    public Vector3 volumeOffset = Vector3.zero;
    
    [Header("Fog Properties")]
    [Range(0, 1)] public float density = 0.5f;
    [Range(0, 1)] public float noiseStrength = 0.5f;
    public Texture3D noiseTexture;
    
    [Header("Rendering")]
    public Material fogMaterial;
    public int resolution = 64;
    
    private ComputeShader _computeShader;
    private RenderTexture _densityVolume;
    private BoxCollider _boundingBox;
    private int _kernelHandle;
    
    private void OnEnable()
    {
        InitializeResources();
        UpdateBounds();
    }
    
    private void OnDisable()
    {
        ReleaseResources();
    }
    
    private void Update()
    {
        if (_computeShader == null) return;
        
        // Update shader properties
        _computeShader.SetFloat("_Density", density);
        _computeShader.SetFloat("_NoiseStrength", noiseStrength);
        _computeShader.SetVector("_VolumeSize", volumeSize);
        _computeShader.SetVector("_VolumeOffset", transform.position + volumeOffset);
        
        // Dispatch compute shader
        int threadGroupsX = Mathf.CeilToInt(resolution / 8f);
        int threadGroupsY = Mathf.CeilToInt(resolution / 8f);
        int threadGroupsZ = Mathf.CeilToInt(resolution / 8f);
        
        _computeShader.Dispatch(_kernelHandle, threadGroupsX, threadGroupsY, threadGroupsZ);
        
        // Update material properties
        if (fogMaterial != null)
        {
            fogMaterial.SetTexture("_DensityVolume", _densityVolume);
            fogMaterial.SetVector("_VolumeBoundsMin", _boundingBox.bounds.min);
            fogMaterial.SetVector("_VolumeBoundsMax", _boundingBox.bounds.max);
        }
    }
    
    private void InitializeResources()
    {
        // Load compute shader
        _computeShader = Resources.Load<ComputeShader>("VolumetricFog");
        if (_computeShader == null)
        {
            Debug.LogError("VolumetricFog compute shader not found in Resources folder!");
            return;
        }
        
        _kernelHandle = _computeShader.FindKernel("FillVolume");
        
        // Create 3D render texture
        _densityVolume = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        _densityVolume.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        _densityVolume.volumeDepth = resolution;
        _densityVolume.enableRandomWrite = true;
        _densityVolume.Create();
        
        // Set shader resources
        _computeShader.SetTexture(_kernelHandle, "_DensityVolume", _densityVolume);
        _computeShader.SetTexture(_kernelHandle, "_NoiseTexture", noiseTexture);
        
        // Get or add box collider for bounds
        _boundingBox = GetComponent<BoxCollider>();
        _boundingBox.isTrigger = true;
    }
    
    private void ReleaseResources()
    {
        if (_densityVolume != null)
        {
            _densityVolume.Release();
            _densityVolume = null;
        }
    }
    
    private void UpdateBounds()
    {
        if (_boundingBox != null)
        {
            _boundingBox.size = volumeSize;
            _boundingBox.center = volumeOffset;
        }
    }
    
    private void OnValidate()
    {
        if (_boundingBox != null)
        {
            UpdateBounds();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.75f, 1f, 0.5f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(volumeOffset, volumeSize);
    }
}