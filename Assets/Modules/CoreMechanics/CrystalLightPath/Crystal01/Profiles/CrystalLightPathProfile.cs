using UnityEngine;

[CreateAssetMenu(fileName = "CrystalLightPathProfile", menuName = "Crystal Light Path/Profile")]
public class CrystalLightPathProfile : ScriptableObject
{
    [Header("Visual Properties")]
    [Range(0.01f, 0.2f)]
    public float width = 0.06f;
    
    [Range(0.1f, 3.0f)]
    public float intensity = 1.3f;
    
    public Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    
    [Header("Hue Range")]
    [Range(-2.0f, 2.0f)]
    public float minimumHue = 0.75f;
    
    [Range(-2.0f, 2.0f)]
    public float maximumHue = 0.95f;
    
    [Header("Fade Settings")]
    [Range(0.1f, 2.0f)]
    public float fadeDistance = 0.5f;
    
    [Header("Profile Info")]
    [TextArea(2, 4)]
    public string description = "Descrição do profile...";
    
    /// <summary>
    /// Aplica todas as propriedades deste profile a um MaterialPropertyBlock
    /// </summary>
    public void ApplyToPropertyBlock(MaterialPropertyBlock propertyBlock)
    {
        propertyBlock.SetFloat("_Width", width);
        propertyBlock.SetFloat("_Intensity", intensity);
        propertyBlock.SetColor("_BackgroundColor", backgroundColor);
        propertyBlock.SetFloat("_Minimum", minimumHue);
        propertyBlock.SetFloat("_Maximum", maximumHue);
        propertyBlock.SetFloat("_FadeDistance", fadeDistance);
    }
    
    /// <summary>
    /// Aplica todas as propriedades deste profile a um Material
    /// </summary>
    public void ApplyToMaterial(Material material)
    {
        material.SetFloat("_Width", width);
        material.SetFloat("_Intensity", intensity);
        material.SetColor("_BackgroundColor", backgroundColor);
        material.SetFloat("_Minimum", minimumHue);
        material.SetFloat("_Maximum", maximumHue);
        material.SetFloat("_FadeDistance", fadeDistance);
    }
    
    /// <summary>
    /// Cria um profile baseado nas propriedades atuais de um Material
    /// </summary>
    public static CrystalLightPathProfile CreateFromMaterial(Material material)
    {
        var profile = CreateInstance<CrystalLightPathProfile>();
        
        profile.width = material.GetFloat("_Width");
        profile.intensity = material.GetFloat("_Intensity");
        profile.backgroundColor = material.GetColor("_BackgroundColor");
        profile.minimumHue = material.GetFloat("_Minimum");
        profile.maximumHue = material.GetFloat("_Maximum");
        profile.fadeDistance = material.GetFloat("_FadeDistance");
        
        return profile;
    }
    
    /// <summary>
    /// Interpola entre dois profiles
    /// </summary>
    public static CrystalLightPathProfile Lerp(CrystalLightPathProfile a, CrystalLightPathProfile b, float t)
    {
        var result = CreateInstance<CrystalLightPathProfile>();
        
        result.width = Mathf.Lerp(a.width, b.width, t);
        result.intensity = Mathf.Lerp(a.intensity, b.intensity, t);
        result.backgroundColor = Color.Lerp(a.backgroundColor, b.backgroundColor, t);
        result.minimumHue = Mathf.Lerp(a.minimumHue, b.minimumHue, t);
        result.maximumHue = Mathf.Lerp(a.maximumHue, b.maximumHue, t);
        result.fadeDistance = Mathf.Lerp(a.fadeDistance, b.fadeDistance, t);
        
        return result;
    }
}