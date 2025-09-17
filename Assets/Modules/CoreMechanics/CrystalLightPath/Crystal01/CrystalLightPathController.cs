using UnityEngine;

[ExecuteAlways]
public class CrystalLightPathController : MonoBehaviour
{
    [Header("Line Renderer")]
    [SerializeField] private LineRenderer lineRenderer;
    
    [Header("Settings")]
    [SerializeField] private bool autoUpdate = true;
    [SerializeField] private bool useDirectMaterialInEditor = true;
    
    private MaterialPropertyBlock propertyBlock;
    private Renderer targetRenderer;
    
    private void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
            
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
            
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }
    
    private void Update()
    {
        if (autoUpdate)
        {
            UpdateShaderPoints();
        }
    }
    
    public void UpdateShaderPoints()
    {
        if (lineRenderer == null || targetRenderer == null) 
        {
            InitializeComponents();
            if (lineRenderer == null || targetRenderer == null) return;
        }
        
        try
        {
            if (lineRenderer.positionCount >= 2)
            {
                Vector3 startPoint = lineRenderer.GetPosition(0);
                Vector3 endPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
                
                // No editor, atualiza material diretamente para visualização em tempo real
                #if UNITY_EDITOR
                if (useDirectMaterialInEditor && !Application.isPlaying)
                {
                    if (targetRenderer.sharedMaterial != null)
                    {
                        var material = targetRenderer.sharedMaterial;
                        material.SetVector("_StartPoint", startPoint);
                        material.SetVector("_EndPoint", endPoint);
                        UnityEditor.EditorUtility.SetDirty(material);
                    }
                    return;
                }
                #endif
                
                // Em runtime, usa PropertyBlock
                if (propertyBlock != null)
                {
                    propertyBlock.SetVector("_StartPoint", startPoint);
                    propertyBlock.SetVector("_EndPoint", endPoint);
                    targetRenderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"CrystalLightPathController: Error updating shader points - {e.Message}");
        }
    }
    
    public void SetFadeDistance(float distance)
    {
        if (targetRenderer == null)
        {
            InitializeComponents();
            if (targetRenderer == null) return;
        }
        
        try
        {
            #if UNITY_EDITOR
            if (useDirectMaterialInEditor && !Application.isPlaying)
            {
                if (targetRenderer.sharedMaterial != null)
                {
                    targetRenderer.sharedMaterial.SetFloat("_FadeDistance", distance);
                    UnityEditor.EditorUtility.SetDirty(targetRenderer.sharedMaterial);
                }
                return;
            }
            #endif
            
            if (propertyBlock != null)
            {
                propertyBlock.SetFloat("_FadeDistance", distance);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"CrystalLightPathController: Error setting fade distance - {e.Message}");
        }
    }
    
    public void SetWidth(float width)
    {
        if (targetRenderer == null || propertyBlock == null)
        {
            InitializeComponents();
            if (targetRenderer == null || propertyBlock == null) return;
        }
        
        try
        {
            propertyBlock.SetFloat("_Width", width);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"CrystalLightPathController: Error setting width - {e.Message}");
        }
    }
    
    public void SetIntensity(float intensity)
    {
        if (targetRenderer == null || propertyBlock == null)
        {
            InitializeComponents();
            if (targetRenderer == null || propertyBlock == null) return;
        }
        
        try
        {
            propertyBlock.SetFloat("_Intensity", intensity);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"CrystalLightPathController: Error setting intensity - {e.Message}");
        }
    }
}