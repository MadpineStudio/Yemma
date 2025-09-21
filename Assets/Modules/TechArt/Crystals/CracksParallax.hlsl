#ifndef CRACKS_PARALLAX_INCLUDED
#define CRACKS_PARALLAX_INCLUDED

// Função de parallax para múltiplas camadas de cracks
// Para usar no Shader Graph:
// 1. Crie um Custom Function node
// 2. Type: File
// 3. Source: CracksParallax.hlsl
// 4. Name: CalculateCracksParallax
float CalculateCracksParallax(
    float2 UV,                      // UV coordinates da superfície
    float3 ViewDirTangent,          // View Direction em tangent space
    float3 NormalMap,               // Normal map (xyz)
    float2 CrackLayers_ST,          // Tiling e Offset (_ST values)
    Texture2D CrackLayers,          // Textura com as camadas de crack (RGB channels)
    SamplerState CrackSampler,      // Sampler para a textura
    float OffsetScale,              // Escala do offset parallax
    float DepthScale,               // Escala da profundidade
    float4 CracksStrength           // Força de cada camada (x=unused, y=G, z=B, w=R)
)
{
    float parallax = 0;
    
    // Aplica tiling e offset nas UVs base
    float2 baseUV = UV * CrackLayers_ST.xy + CrackLayers_ST.zw;
    
    // Loop para 4 camadas de parallax
    for (int j = 0; j < 4; j++)
    {
        float ratio = (float)j / 4.0;
        
        if (j == 0)
        {
            // Primeira layer é ignorada (seria flat, sem profundidade)
            continue;
        }
        else if (j == 1)
        {
            // Segunda layer - Canal G (mais próxima da superfície)
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = baseUV + parallaxOffset;
            float4 crackSample = CrackLayers.Sample(CrackSampler, offsetUV);
            parallax += crackSample.g * CracksStrength.y;
        }
        else if (j == 2)
        {
            // Terceira layer - Canal B (profundidade média)
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = baseUV + parallaxOffset;
            float4 crackSample = CrackLayers.Sample(CrackSampler, offsetUV);
            parallax += crackSample.b * CracksStrength.z;
        }
        else if (j == 3)
        {
            // Quarta layer - Canal R (mais profunda)
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = baseUV + parallaxOffset;
            float4 crackSample = CrackLayers.Sample(CrackSampler, offsetUV);
            parallax += crackSample.r * CracksStrength.w;
        }
    }
    
    // Multiplica o resultado final
    parallax *= 1.5;
    return parallax;
}

// Versão simplificada para usar com nós do Shader Graph separados
void CalculateCracksParallax_float(
    float2 UV,
    float3 ViewDirTangent,
    float3 NormalMap,
    UnityTexture2D CrackLayers,
    float OffsetScale,
    float DepthScale,
    float4 CracksStrength,
    out float Parallax
)
{
    float parallax = 0;
    
    // Loop para 4 camadas de parallax
    for (int j = 0; j < 4; j++)
    {
        float ratio = (float)j / 4.0;
        
        if (j == 0)
        {
            // Primeira layer é ignorada
            continue;
        }
        else if (j == 1)
        {
            // Canal G
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = UV + parallaxOffset;
            float4 crackSample = SAMPLE_TEXTURE2D(CrackLayers.tex, CrackLayers.samplerstate, offsetUV);
            parallax += crackSample.g * CracksStrength.y;
        }
        else if (j == 2)
        {
            // Canal B
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = UV + parallaxOffset;
            float4 crackSample = SAMPLE_TEXTURE2D(CrackLayers.tex, CrackLayers.samplerstate, offsetUV);
            parallax += crackSample.b * CracksStrength.z;
        }
        else if (j == 3)
        {
            // Canal R
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = UV + parallaxOffset;
            float4 crackSample = SAMPLE_TEXTURE2D(CrackLayers.tex, CrackLayers.samplerstate, offsetUV);
            parallax += crackSample.r * CracksStrength.w;
        }
    }
    
    parallax *= 1.5;
    Parallax = parallax;
}

#endif // CRACKS_PARALLAX_INCLUDED