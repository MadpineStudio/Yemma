Shader "Custom/S_CustomLit"
{
    Properties
    {
        [Header(Base Material)]
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [Normal] _NormalMap("Normal Map", 2D) = "bump" {}
        _PhysicsMap("Physics Map (R=Rough, G=Metal, B=AO)", 2D) = "white" {}
        _BaseSpecular("Base Reflectance", Range(0, 1)) = 0.04
        
        // URP Smoothness System
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
        [Space(10)]

        [Header(Layer Blending)]
        _MaskMap("Layer Mask (R=L0, G=L1, B=L2, A=L3)", 2D) = "black" {}
        [Space(10)]

        [Header(Layer 0  Red Channel)]
        [Toggle(USE_LAYER_R)] _UseLayerR("Enable Layer R", Float) = 0
        _LayerRColor("Albedo Tint", Color) = (1, 1, 1, 1)
        _LayerR_Map("Albedo Map", 2D) = "white" {}
        _LayerR_NormalMap("Normal Map", 2D) = "bump" {}
        _LayerR_PhysicsMap("Physics (R=Rough, G=Metal, B=AO)", 2D) = "white" {}
        _LayerRSpecular("Reflectance", Range(0, 1)) = 0.04
        [Space(5)]

        [Header(Layer 1  Green Channel)]
        [Toggle(USE_LAYER_G)] _UseLayerG("Enable Layer G", Float) = 0
        _LayerGColor("Albedo Tint", Color) = (1, 1, 1, 1)
        _LayerG_Map("Albedo Map", 2D) = "white" {}
        _LayerG_NormalMap("Normal Map", 2D) = "bump" {}
        _LayerG_PhysicsMap("Physics (R=Rough, G=Metal, B=AO)", 2D) = "white" {}
        _LayerGSpecular("Reflectance", Range(0, 1)) = 0.04
        [Space(5)]

        [Header(Layer 2  Blue Channel)]
        [Toggle(USE_LAYER_B)] _UseLayerB("Enable Layer B", Float) = 0
        _LayerBColor("Albedo Tint", Color) = (1, 1, 1, 1)
        _LayerB_Map("Albedo Map", 2D) = "white" {}
        _LayerB_NormalMap("Normal Map", 2D) = "bump" {}
        _LayerB_PhysicsMap("Physics (R=Rough, G=Metal, B=AO)", 2D) = "white" {}
        _LayerBSpecular("Reflectance", Range(0, 1)) = 0.04
        [Space(5)]

        [Header(Layer 3  Alpha Channel)]
        [Toggle(USE_LAYER_A)] _UseLayerA("Enable Layer A", Float) = 0
        _LayerAColor("Albedo Tint", Color) = (1, 1, 1, 1)
        _LayerA_Map("Albedo Map", 2D) = "white" {}
        _LayerA_NormalMap("Normal Map", 2D) = "bump" {}
        _LayerA_PhysicsMap("Physics (R=Rough, G=Metal, B=AO)", 2D) = "white" {}
        _LayerASpecular("Reflectance", Range(0, 1)) = 0.04
        [Space(10)]

        [Header(UV Controls)]
        _Tiling("Tiling", Vector) = (1, 1, 0, 0)
        _Offset("Offset", Vector) = (0, 0, 0, 0)
        [Space(10)]

        [Header(Global Adjustments)]
        _NormalStrength("Normal Strength", Range(0, 2)) = 1
        _RoughAdd("Roughness Modifier", Range(-1, 1)) = 0
        _MetalAdd("Metallic Modifier", Range(-1, 1)) = 0
        [Space(5)]
        
        [Header(Lighting Controls)]
        _SpecularIntensity("Specular Intensity", Range(0, 5)) = 1.5
        _SpecularPower("Specular Power", Range(8, 1024)) = 256
        _FresnelPower("Fresnel Power", Range(0, 10)) = 5
        _MetallicBoost("Metallic Brightness Boost", Range(1, 10)) = 3
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // Multi-compile directives for lighting
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _MASKMAP
            #pragma shader_feature_local USE_LAYER_R
            #pragma shader_feature_local USE_LAYER_G
            #pragma shader_feature_local USE_LAYER_B
            #pragma shader_feature_local USE_LAYER_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
            };

            TEXTURE2D(_BaseMap);
            TEXTURE2D(_MaskMap);
            TEXTURE2D(_PhysicsMap);
            TEXTURE2D(_NormalMap);
            TEXTURE2D(_MetallicGlossMap);
            TEXTURE2D(_LayerR_Map);
            TEXTURE2D(_LayerR_NormalMap);
            TEXTURE2D(_LayerR_PhysicsMap);
            TEXTURE2D(_LayerG_Map);
            TEXTURE2D(_LayerG_NormalMap);
            TEXTURE2D(_LayerG_PhysicsMap);
            TEXTURE2D(_LayerB_Map);
            TEXTURE2D(_LayerB_NormalMap);
            TEXTURE2D(_LayerB_PhysicsMap);
            TEXTURE2D(_LayerA_Map);
            TEXTURE2D(_LayerA_NormalMap);
            TEXTURE2D(_LayerA_PhysicsMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _LayerRColor;
                half4 _LayerGColor;
                half4 _LayerBColor;
                half4 _LayerAColor;
                float4 _Tiling;
                float4 _Offset;
                half _NormalStrength;
                half _RoughAdd;
                half _MetalAdd;
                half _BaseSpecular;
                half _LayerRSpecular;
                half _LayerGSpecular;
                half _LayerBSpecular;
                half _LayerASpecular;
                half _SpecularIntensity;
                half _SpecularPower;
                half _FresnelPower;
                half _MetallicBoost;
                // URP Smoothness
                half _Smoothness;
                half _SmoothnessTextureChannel;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                
                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.tangentWS = float4(normalInputs.tangentWS, IN.tangentOS.w);
                OUT.viewDirWS = GetWorldSpaceViewDir(OUT.positionWS);
                OUT.uv = IN.uv * _Tiling.xy + _Offset.xy;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Sample base material with URP smoothness system
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
                half4 basePhysics = SAMPLE_TEXTURE2D(_PhysicsMap, sampler_BaseMap, uv);
                half3 baseNormal = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, uv), _NormalStrength);

                // URP Metallic/Smoothness sampling
                half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_BaseMap, uv);
                half baseSmoothness = _Smoothness;
                half baseMetallic = metallicGloss.r;
                
                // Apply smoothness from texture channel
                #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
                    baseSmoothness = baseColor.a * _Smoothness;
                #else
                    if (_SmoothnessTextureChannel == 0)
                        baseSmoothness = metallicGloss.a * _Smoothness;
                    else
                        baseSmoothness = metallicGloss.r * _Smoothness;
                #endif

                // Initialize blended properties
                half3 finalColor = baseColor.rgb;
                half roughness = 1.0 - baseSmoothness;  // Convert smoothness to roughness
                half metallic = baseMetallic;
                half ao = basePhysics.b;
                half specular = _BaseSpecular;

                // Layer blending (only for color, roughness, metallic, AO, specular)
                #if defined(USE_LAYER_R) || defined(USE_LAYER_G) || defined(USE_LAYER_B) || defined(USE_LAYER_A)
                    half4 m = SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, uv);
                    half wR=0,wG=0,wB=0,wA=0;
                    #ifdef USE_LAYER_R
                        wR = m.r;
                    #endif
                    #ifdef USE_LAYER_G
                        wG = m.g;
                    #endif
                    #ifdef USE_LAYER_B
                        wB = m.b;
                    #endif
                    #ifdef USE_LAYER_A
                        wA = m.a;
                    #endif
                    half w0 = saturate(1.0 - saturate(wR + wG + wB + wA));
                    half sum = max(w0 + wR + wG + wB + wA, 1e-3);
                    w0/=sum; wR/=sum; wG/=sum; wB/=sum; wA/=sum;

                    finalColor = w0 * baseColor.rgb;
                    roughness = w0 * (1.0 - baseSmoothness);
                    metallic = w0 * baseMetallic;
                    ao = w0 * basePhysics.b;
                    specular = w0 * _BaseSpecular;

                    #ifdef USE_LAYER_R
                        half3 cR = SAMPLE_TEXTURE2D(_LayerR_Map, sampler_BaseMap, uv).rgb * _LayerRColor.rgb;
                        half4 pR = SAMPLE_TEXTURE2D(_LayerR_PhysicsMap, sampler_BaseMap, uv);
                        finalColor += wR * cR;
                        roughness += wR * pR.r;  // Use roughness from physics map
                        metallic += wR * pR.g;
                        ao += wR * pR.b;
                        specular += wR * _LayerRSpecular;
                    #endif
                    #ifdef USE_LAYER_G
                        half3 cG = SAMPLE_TEXTURE2D(_LayerG_Map, sampler_BaseMap, uv).rgb * _LayerGColor.rgb;
                        half4 pG = SAMPLE_TEXTURE2D(_LayerG_PhysicsMap, sampler_BaseMap, uv);
                        finalColor += wG * cG;
                        roughness += wG * pG.r;
                        metallic += wG * pG.g;
                        ao += wG * pG.b;
                        specular += wG * _LayerGSpecular;
                    #endif
                    #ifdef USE_LAYER_B
                        half3 cB = SAMPLE_TEXTURE2D(_LayerB_Map, sampler_BaseMap, uv).rgb * _LayerBColor.rgb;
                        half4 pB = SAMPLE_TEXTURE2D(_LayerB_PhysicsMap, sampler_BaseMap, uv);
                        finalColor += wB * cB;
                        roughness += wB * pB.r;
                        metallic += wB * pB.g;
                        ao += wB * pB.b;
                        specular += wB * _LayerBSpecular;
                    #endif
                    #ifdef USE_LAYER_A
                        half3 cA = SAMPLE_TEXTURE2D(_LayerA_Map, sampler_BaseMap, uv).rgb * _LayerAColor.rgb;
                        half4 pA = SAMPLE_TEXTURE2D(_LayerA_PhysicsMap, sampler_BaseMap, uv);
                        finalColor += wA * cA;
                        roughness += wA * pA.r;
                        metallic += wA * pA.g;
                        ao += wA * pA.b;
                        specular += wA * _LayerASpecular;
                    #endif
                #endif

                // Apply global adjustments
                roughness = saturate(roughness + _RoughAdd);
                metallic = saturate(metallic + _MetalAdd);

                // Convert roughness back to smoothness for proper lighting
                half smoothness = saturate(1.0 - roughness);

                // Transform normal to world space using TBN matrix - Fixed
                float3 normalWS = normalize(IN.normalWS);
                float3 tangentWS = normalize(IN.tangentWS.xyz);
                float3 bitangentWS = normalize(cross(normalWS, tangentWS) * IN.tangentWS.w);
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);

                // Base normal segura
                half3 baseNrmTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                float3 baseNormalWS = SafeNormalize(mul(baseNrmTS, TBN));
                float3 finalNormalWS = baseNormalWS;

                // Blend normals in world space for better results
                #if defined(USE_LAYER_R) || defined(USE_LAYER_G) || defined(USE_LAYER_B) || defined(USE_LAYER_A)
                    half4 m = SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, uv);
                    half wR=0,wG=0,wB=0,wA=0;
                    #ifdef USE_LAYER_R
                        wR = m.r;
                    #endif
                    #ifdef USE_LAYER_G
                        wG = m.g;
                    #endif
                    #ifdef USE_LAYER_B
                        wB = m.b;
                    #endif
                    #ifdef USE_LAYER_A
                        wA = m.a;
                    #endif
                    half w0 = saturate(1.0 - saturate(wR + wG + wB + wA));
                    half sum = max(w0 + wR + wG + wB + wA, 1e-3);
                    w0/=sum; wR/=sum; wG/=sum; wB/=sum; wA/=sum;

                    finalNormalWS = w0 * baseNormalWS;

                    #ifdef USE_LAYER_R
                        half3 nR = UnpackNormalScale(SAMPLE_TEXTURE2D(_LayerR_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                        float3 nRWS = SafeNormalize(mul(nR, TBN));
                        finalNormalWS += wR * nRWS;
                    #endif
                    #ifdef USE_LAYER_G
                        half3 nG = UnpackNormalScale(SAMPLE_TEXTURE2D(_LayerG_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                        float3 nGWS = SafeNormalize(mul(nG, TBN));
                        finalNormalWS += wG * nGWS;
                    #endif
                    #ifdef USE_LAYER_B
                        half3 nB = UnpackNormalScale(SAMPLE_TEXTURE2D(_LayerB_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                        float3 nBWS = SafeNormalize(mul(nB, TBN));
                        finalNormalWS += wB * nBWS;
                    #endif
                    #ifdef USE_LAYER_A
                        half3 nA = UnpackNormalScale(SAMPLE_TEXTURE2D(_LayerA_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                        float3 nAWS = SafeNormalize(mul(nA, TBN));
                        finalNormalWS += wA * nAWS;
                    #endif

                    finalNormalWS = SafeNormalize(finalNormalWS);
                #endif

                // Initialize lighting
                half3 color = 0;
                
                // Setup common lighting variables
                float3 V = SafeNormalize(GetWorldSpaceViewDir(IN.positionWS));
                half gloss = smoothness;  // Use smoothness directly
                half shin = lerp(16.0, _SpecularPower, gloss * gloss);       // Shininess controlável
                
                // F0 melhorado para metálicos com modifier aplicado
                half3 F0 = lerp(half3(0.04, 0.04, 0.04), finalColor, metallic);
                half specularIntensity = lerp(_SpecularIntensity, _SpecularIntensity * _MetallicBoost, metallic);  // Boost controlável
                
                // Main directional light (com sombra)
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
                half NdotL = saturate(dot(finalNormalWS, mainLight.direction));
                half3 diff = finalColor * mainLight.color * NdotL * mainLight.shadowAttenuation;
                color += diff;
                
                // Specular melhorado para main light
                float3 L = mainLight.direction;
                float3 H = SafeNormalize(L + V);
                half NdotH = saturate(dot(finalNormalWS, H));
                half VdotH = saturate(dot(V, H));
                
                // Fresnel controlável
                half fresnel = F0 + (1.0 - F0) * pow(1.0 - VdotH, _FresnelPower);
                half specTerm = pow(NdotH, shin) * fresnel * specularIntensity;
                half3 specMain = specTerm * mainLight.color * mainLight.shadowAttenuation * NdotL;
                color += specMain;
                
                // Additional lights (point/spot lights)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, IN.positionWS);
                    
                    // Calculate proper light direction and NdotL
                    half NdotL_add = saturate(dot(finalNormalWS, light.direction));
                    
                    // Diffuse contribution
                    half3 lightContrib = light.color * NdotL_add * light.distanceAttenuation * light.shadowAttenuation;
                    color += lightContrib * finalColor;
                    
                    // Specular melhorado para additional lights
                    float3 La = light.direction;
                    float3 Ha = SafeNormalize(La + V);
                    half NdotH_add = saturate(dot(finalNormalWS, Ha));
                    half VdotH_add = saturate(dot(V, Ha));
                    
                    // Fresnel controlável para additional lights
                    half fresnelAdd = F0 + (1.0 - F0) * pow(1.0 - VdotH_add, _FresnelPower);
                    half specA = pow(NdotH_add, shin) * fresnelAdd * specularIntensity;
                    half3 specContrib = specA * light.color * NdotL_add * light.distanceAttenuation * light.shadowAttenuation;
                    color += specContrib;
                }
                
                // Ambient lighting
                color += SampleSH(finalNormalWS) * finalColor * ao;
                
                return half4(color, baseColor.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            ShadowVaryings ShadowPassVertex(ShadowAttributes input)
            {
                ShadowVaryings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                output.positionCS = positionCS;
                return output;
            }

            half4 ShadowPassFragment(ShadowVaryings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            DepthVaryings DepthOnlyVertex(DepthAttributes input)
            {
                DepthVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(DepthVaryings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}



