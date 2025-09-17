using UnityEngine;
using System.Collections;

public class CrystalLightPathInterpolator : MonoBehaviour
{
    [Header("Estados")]
    [SerializeField] private CrystalLightPathProfile naoDetectadoProfile;
    [SerializeField] private CrystalLightPathProfile detectadoProfile;
    
    [Header("Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve interpolationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Debug")]
    [SerializeField] private bool useDirectMaterialInEditor = true;
    
    private MaterialPropertyBlock propertyBlock;
    private Renderer targetRenderer;
    private CrystalLightPathController lightPathController;
    private Coroutine currentTransition;
    private bool isDetectado = false;
    
    // Valores atuais para interpolação suave
    private float currentWidth;
    private float currentIntensity;
    private Color currentBackgroundColor;
    private float currentMinimumHue;
    private float currentMaximumHue;
    private float currentFadeDistance;
    
    private void Start()
    {
        InitializeComponents();
        InitializeCurrentValues();
        // Inicia no estado não detectado
        NaoDetectado();
    }
    
    private void Update()
    {
        // Atualiza pontos de geometria imediatamente a cada frame
        UpdateGeometryPoints();
    }
    
    private void InitializeCurrentValues()
    {
        if (naoDetectadoProfile != null)
        {
            currentWidth = naoDetectadoProfile.width;
            currentIntensity = naoDetectadoProfile.intensity;
            currentBackgroundColor = naoDetectadoProfile.backgroundColor;
            currentMinimumHue = naoDetectadoProfile.minimumHue;
            currentMaximumHue = naoDetectadoProfile.maximumHue;
            currentFadeDistance = naoDetectadoProfile.fadeDistance;
        }
    }
    
    private void InitializeComponents()
    {
        if (lightPathController == null)
            lightPathController = GetComponent<CrystalLightPathController>();
            
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
            
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }
    
    /// <summary>
    /// Muda para estado detectado
    /// </summary>
    public void Detectado()
    {
        if (!isDetectado && detectadoProfile != null)
        {
            isDetectado = true;
            
            // Atualiza pontos geométricos imediatamente
            UpdateGeometryPointsImmediate();
            
            // Inicia interpolação dos parâmetros visuais
            TransitionToProfile(detectadoProfile);
        }
    }
    
    /// <summary>
    /// Muda para estado não detectado
    /// </summary>
    public void NaoDetectado()
    {
        if (isDetectado && naoDetectadoProfile != null)
        {
            isDetectado = false;
            
            // Atualiza pontos geométricos imediatamente
            UpdateGeometryPointsImmediate();
            
            // Inicia interpolação dos parâmetros visuais
            TransitionToProfile(naoDetectadoProfile);
        }
        else if (!isDetectado && naoDetectadoProfile != null && currentTransition == null)
        {
            // Primeira inicialização
            UpdateGeometryPointsImmediate();
            ApplyProfileImmediate(naoDetectadoProfile);
        }
    }
    
    /// <summary>
    /// Obtém estado atual
    /// </summary>
    public bool IsDetectado() => isDetectado;
    
    private void TransitionToProfile(CrystalLightPathProfile targetProfile)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
            
        currentTransition = StartCoroutine(TransitionCoroutine(targetProfile));
    }
    
    private void ApplyProfileImmediate(CrystalLightPathProfile profile)
    {
        InitializeComponents();
        
        // Atualiza valores atuais
        currentWidth = profile.width;
        currentIntensity = profile.intensity;
        currentBackgroundColor = profile.backgroundColor;
        currentMinimumHue = profile.minimumHue;
        currentMaximumHue = profile.maximumHue;
        currentFadeDistance = profile.fadeDistance;
        
        // Aplica ao PropertyBlock
        ApplyCurrentValuesToPropertyBlock();
    }
    
    private IEnumerator TransitionCoroutine(CrystalLightPathProfile targetProfile)
    {
        InitializeComponents();
        
        if (targetRenderer == null || propertyBlock == null) yield break;
        
        // Pega valores atuais do MaterialPropertyBlock
        var startProfile = GetCurrentProfileFromMaterial();
        
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / transitionDuration;
            float curveTime = interpolationCurve.Evaluate(normalizedTime);
            
            // Interpola valores frame a frame
            ApplyInterpolatedProfile(startProfile, targetProfile, curveTime);
            
            yield return null;
        }
        
        // Garante valores finais exatos
        ApplyProfileImmediate(targetProfile);
        currentTransition = null;
    }
    
    private CrystalLightPathProfile GetCurrentProfileFromMaterial()
    {
        var current = ScriptableObject.CreateInstance<CrystalLightPathProfile>();
        
        // Usa os valores atuais que estão sendo interpolados
        current.width = currentWidth;
        current.intensity = currentIntensity;
        current.backgroundColor = currentBackgroundColor;
        current.minimumHue = currentMinimumHue;
        current.maximumHue = currentMaximumHue;
        current.fadeDistance = currentFadeDistance;
        
        return current;
    }
    
    private void ApplyInterpolatedProfile(CrystalLightPathProfile from, CrystalLightPathProfile to, float t)
    {
        // Interpola valores e atualiza variáveis atuais
        currentWidth = Mathf.Lerp(from.width, to.width, t);
        currentIntensity = Mathf.Lerp(from.intensity, to.intensity, t);
        currentBackgroundColor = Color.Lerp(from.backgroundColor, to.backgroundColor, t);
        currentMinimumHue = Mathf.Lerp(from.minimumHue, to.minimumHue, t);
        currentMaximumHue = Mathf.Lerp(from.maximumHue, to.maximumHue, t);
        currentFadeDistance = Mathf.Lerp(from.fadeDistance, to.fadeDistance, t);
        
        // Aplica os valores interpolados ao PropertyBlock
        ApplyCurrentValuesToPropertyBlock();
    }
    
    private void ApplyCurrentValuesToPropertyBlock()
    {
        if (targetRenderer == null) return;
        
        // No editor, usa material diretamente para ver mudanças em tempo real
        #if UNITY_EDITOR
        if (useDirectMaterialInEditor && !Application.isPlaying)
        {
            ApplyToMaterialDirect();
            return;
        }
        #endif
        
        // Em runtime, usa PropertyBlock para performance
        if (propertyBlock == null) return;
        
        // Aplica apenas parâmetros visuais (não geométricos)
        propertyBlock.SetFloat("_Width", currentWidth);
        propertyBlock.SetFloat("_Intensity", currentIntensity);
        propertyBlock.SetColor("_BackgroundColor", currentBackgroundColor);
        propertyBlock.SetFloat("_Minimum", currentMinimumHue);
        propertyBlock.SetFloat("_Maximum", currentMaximumHue);
        propertyBlock.SetFloat("_FadeDistance", currentFadeDistance);
        
        targetRenderer.SetPropertyBlock(propertyBlock);
    }
    
    #if UNITY_EDITOR
    private void ApplyToMaterialDirect()
    {
        if (targetRenderer.sharedMaterial == null) return;
        
        var material = targetRenderer.sharedMaterial;
        
        material.SetFloat("_Width", currentWidth);
        material.SetFloat("_Intensity", currentIntensity);
        material.SetColor("_BackgroundColor", currentBackgroundColor);
        material.SetFloat("_Minimum", currentMinimumHue);
        material.SetFloat("_Maximum", currentMaximumHue);
        material.SetFloat("_FadeDistance", currentFadeDistance);
        
        // Marca como dirty para o editor atualizar
        UnityEditor.EditorUtility.SetDirty(material);
    }
    #endif
    
    private void UpdateGeometryPoints()
    {
        if (propertyBlock == null || targetRenderer == null || lightPathController == null) return;
        
        // Força atualização dos pontos geométricos via CrystalLightPathController
        lightPathController.UpdateShaderPoints();
    }
    
    private void UpdateGeometryPointsImmediate()
    {
        if (lightPathController != null)
        {
            // Força atualização imediata dos pontos StartPoint e EndPoint
            lightPathController.UpdateShaderPoints();
        }
    }
    
    private void CopyProfile(CrystalLightPathProfile source, CrystalLightPathProfile destination)
    {
        destination.width = source.width;
        destination.intensity = source.intensity;
        destination.backgroundColor = source.backgroundColor;
        destination.minimumHue = source.minimumHue;
        destination.maximumHue = source.maximumHue;
        destination.fadeDistance = source.fadeDistance;
    }
}