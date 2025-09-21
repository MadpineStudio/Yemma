#ifndef CRACKS_PARALLAX_INCLUDED
#define CRACKS_PARALLAX_INCLUDED

// Função de parallax para múltiplas camadas de cracks - APENAS SHADER GRAPH
// Para usar:
// 1. Custom Function node
// 2. Type: File
// 3. Source: CracksParallax.hlsl
// 4. Name: CalculateCracksParallax_float

void CalculateCracksParallax_float(
    float2 UV,                      // UV coordinates
    float3 ViewDirTangent,          // View Direction em tangent space
    float3 NormalMap,               // Normal map (xyz)
    UnityTexture2D CrackLayers,     // Textura com camadas RGB
    float OffsetScale,              // Escala do offset parallax
    float DepthScale,               // Escala da profundidade
    float4 CracksStrength,          // Força (x=unused, y=G, z=B, w=R)
    out float Parallax              // Output
)
{
    float parallax = 0;
    
    // Loop para 4 camadas de parallax
    for (int j = 0; j < 4; j++)
    {
        float ratio = (float)j / 4.0;
        
        if (j == 0)
        {
            // Primeira layer é ignorada (flat)
            continue;
        }
        else if (j == 1)
        {
            // Canal G - mais próxima da superfície
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = UV + parallaxOffset;
            float4 crackSample = SAMPLE_TEXTURE2D(CrackLayers.tex, CrackLayers.samplerstate, offsetUV);
            parallax += crackSample.g * CracksStrength.y;
        }
        else if (j == 2)
        {
            // Canal B - profundidade média
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = UV + parallaxOffset;
            float4 crackSample = SAMPLE_TEXTURE2D(CrackLayers.tex, CrackLayers.samplerstate, offsetUV);
            parallax += crackSample.b * CracksStrength.z;
        }
        else if (j == 3)
        {
            // Canal R - mais profunda
            float depthRatio = ratio * DepthScale;
            float2 parallaxOffset = lerp(0, OffsetScale, depthRatio) * ViewDirTangent.xy + NormalMap.xy * 0.01;
            float2 offsetUV = UV + parallaxOffset;
            float4 crackSample = SAMPLE_TEXTURE2D(CrackLayers.tex, CrackLayers.samplerstate, offsetUV);
            parallax += crackSample.r * CracksStrength.w;
        }
    }
    
    // Resultado final
    Parallax = parallax * 1.5;
}

#endif // CRACKS_PARALLAX_INCLUDED