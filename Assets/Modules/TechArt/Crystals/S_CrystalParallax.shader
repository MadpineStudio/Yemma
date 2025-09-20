Shader "Custom/S_CrystalParallax"
{
    Properties
    {
        _IceTint("Ice Texture Tint", Color) = (1,1,1,1)
        _MainTex("Ice Albedo (RGB)", 2D) = "white" {}
        _CrackLayers("Packed Cracks Texture", 2D) = "white" {}
        _OffsetScale("Crack Offset Scale", float) = 0.5
        _DepthScale("Depth Scale", Range(0.0, 2.0)) = 1.0
        _CracksStrength("Cracks Fade Strength", Vector) = (0.0, 0.75, 0.45, 0.25)
        _NormalTex("Ice Normal Texture", 2D) = "bump" {}
        _Roughness("Ice Roughness Texture", 2D) = "black" {}
        _RoughnessStrength("Roughness Strength", Range(0.0, 1.0)) = 0.4
        _Metallic("Metallic", Range(0,1)) = 0.0
        _ReflectionDistortion("Reflection Distortion", Range(0.0, 0.2)) = 0.05
        _ReflectionTint("Reflection Tint", Color) = (1, 1, 1, 1)
        _FresnelPower("Fresnel Power", Range(0.1, 5.0)) = 1.0
        _FresnelIntensity("Fresnel Intensity", Range(0.0, 2.0)) = 1.0
        _FresnelBias("Fresnel Bias", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDirTangent : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_CrackLayers);
            SAMPLER(sampler_CrackLayers);
            
            TEXTURE2D(_NormalTex);
            SAMPLER(sampler_NormalTex);
            
            TEXTURE2D(_Roughness);
            SAMPLER(sampler_Roughness);
            
            TEXTURE2D(_ReflectionMap);
            SAMPLER(sampler_ReflectionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _CrackLayers_ST;
                float4 _NormalTex_ST;
                float4 _Roughness_ST;
                float4 _IceTint;
                float _OffsetScale;
                float _DepthScale;
                float4 _CracksStrength;
                float _RoughnessStrength;
                float _Metallic;
                float _ReflectionDistortion;
                half4 _ReflectionTint;
                float _FresnelPower;
                float _FresnelIntensity;
                float _FresnelBias;
            CBUFFER_END

            // Função de blend multiply do shader original
            float4 blendMultiply(float4 baseTex, float4 blendTex, float opacity)
            {
                float4 baseBlend = baseTex * blendTex;
                float4 ret = lerp(baseTex, baseBlend, opacity);
                return ret;
            }

            // Função Fresnel
            float CalculateFresnel(float3 normalWS, float3 viewDirWS, float power, float intensity, float bias)
            {
                float NdotV = saturate(dot(normalize(normalWS), normalize(viewDirWS)));
                float fresnel = bias + (1.0 - bias) * pow(1.0 - NdotV, power);
                return fresnel * intensity;
            }

            // Função de parallax com controle de profundidade
            float CalculateCracksParallax(float2 uv_CrackLayers, float3 viewDirTangent, float3 normalTex)
            {
                float parallax = 0;
                
                for (int j = 0; j < 4; j++)
                {
                    float ratio = (float)j / 4;
                    
                    if (j == 0)
                    {
                        // Não mostra a primeira layer, pois seria flat no objeto (sem profundidade)
                        // Queremos começar com a segunda iteração do efeito parallax
                    }
                    else if (j == 1)
                    {
                        // Primeira layer de cracks (canal G) - mais próxima da superfície
                        float depthRatio = ratio * _DepthScale;
                        float2 offsetUV = uv_CrackLayers + lerp(0, _OffsetScale, depthRatio) * viewDirTangent.xy + normalTex.xy * 0.01;
                        parallax += SAMPLE_TEXTURE2D(_CrackLayers, sampler_CrackLayers, offsetUV).g * _CracksStrength.y;
                    }
                    else if (j == 2)
                    {
                        // Segunda layer de cracks (canal B) - profundidade média
                        float depthRatio = ratio * _DepthScale;
                        float2 offsetUV = uv_CrackLayers + lerp(0, _OffsetScale, depthRatio) * viewDirTangent.xy + normalTex.xy * 0.01;
                        parallax += SAMPLE_TEXTURE2D(_CrackLayers, sampler_CrackLayers, offsetUV).b * _CracksStrength.z;
                    }
                    else if (j == 3)
                    {
                        // Terceira layer de cracks (canal R) - mais profunda
                        float depthRatio = ratio * _DepthScale;
                        float2 offsetUV = uv_CrackLayers + lerp(0, _OffsetScale, depthRatio) * viewDirTangent.xy + normalTex.xy * 0.01;
                        parallax += SAMPLE_TEXTURE2D(_CrackLayers, sampler_CrackLayers, offsetUV).r * _CracksStrength.w;
                    }
                }
                
                parallax *= 1.5;
                return parallax;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                
                // [VIEW DIRECTION IN TANGENT SPACE]: Para que o parallax funcione corretamente,
                // precisamos encontrar a direção de view da câmera no tangent space
                // Cálculo abaixo cuida disso. Crédito: Harry Alisavakis
                float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
                float3 viewDir = IN.positionOS.xyz - objCam.xyz;
                float tangentSign = IN.tangentOS.w * unity_WorldTransformParams.w;
                float3 bitangent = cross(IN.normalOS.xyz, IN.tangentOS.xyz) * tangentSign;
                
                // Normaliza as direções para evitar distorção
                float3 normalizedViewDir = normalize(viewDir);
                float3 normalizedTangent = normalize(IN.tangentOS.xyz);
                float3 normalizedBitangent = normalize(bitangent);
                float3 normalizedNormal = normalize(IN.normalOS.xyz);
                
                OUT.viewDirTangent = float3(
                    dot(normalizedViewDir, normalizedTangent),
                    dot(normalizedViewDir, normalizedBitangent),
                    dot(normalizedViewDir, normalizedNormal)
                );
                
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                
                // World space normal e view direction para Fresnel
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = _WorldSpaceCameraPos - positionWS;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Screen UV para reflexão
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                
                // Amostra as texturas exatamente como no shader original
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(IN.uv, _MainTex)) * _IceTint;
                half3 normalTex = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, TRANSFORM_TEX(IN.uv, _NormalTex)));
                half roughnessTex = SAMPLE_TEXTURE2D(_Roughness, sampler_Roughness, TRANSFORM_TEX(IN.uv, _Roughness)).r * _RoughnessStrength;
                
                // Aplica distorção das normais nas coordenadas de reflexão
                float2 refractionOffset = normalTex.xy * _ReflectionDistortion;
                float2 distortedScreenUV = screenUV + refractionOffset;
                
                // Reflexão com distorção pelas normais
                float3 reflection = SAMPLE_TEXTURE2D(_ReflectionMap, sampler_ReflectionMap, distortedScreenUV).rgb;
                
                // Aplica tint de cor na reflexão
                reflection *= _ReflectionTint.rgb;
                
                // Calcula Fresnel
                float fresnel = CalculateFresnel(IN.normalWS, IN.viewDirWS, _FresnelPower, _FresnelIntensity, _FresnelBias);
                
                // Aplica Fresnel na reflexão
                reflection *= fresnel;
                
                // Calcula o parallax das cracks exatamente como no shader original
                float parallax = CalculateCracksParallax(TRANSFORM_TEX(IN.uv, _CrackLayers), IN.viewDirTangent, normalTex);
                
                // Aplica o blend multiply como no shader original
                half4 blended = blendMultiply(mainTex, parallax, 0.55);
                
                // Simula as propriedades PBR do shader original
                // No URP, aplicamos diretamente no albedo já que não temos surface shader
                half smoothness = 1.0 - roughnessTex;
                half metallic = _Metallic;
                
                // Combina metallic e smoothness no resultado final
                // Para uma implementação completa, seria necessário usar um surface shader ou forward rendering
                half4 finalColor = blended;
                finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * metallic, metallic);
                finalColor.rgb *= smoothness + 0.5; // Simula o efeito de smoothness
                
                // Adiciona reflexão
                finalColor.rgb += reflection;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
