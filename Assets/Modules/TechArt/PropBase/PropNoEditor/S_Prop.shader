Shader "Custom/teste"
{
    Properties
    {
        [Header(Base Material)]
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [Normal] _NormalMap("Normal Map", 2D) = "bump" {}
        _PhysicsMap("Physics Map (R=Rough, G=Metal, B=AO)", 2D) = "white" {}
        _BaseSpecular("Base Reflectance", Range(0, 1)) = 0.04
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
    }

    CustomEditor "S_PropEditor"

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _MASKMAP
            #pragma shader_feature_local USE_LAYER_R
            #pragma shader_feature_local USE_LAYER_G
            #pragma shader_feature_local USE_LAYER_B
            #pragma shader_feature_local USE_LAYER_A

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            // Force additional lights support
            #if !defined(_ADDITIONAL_LIGHTS) && !defined(_ADDITIONAL_LIGHTS_VERTEX)
                #define _ADDITIONAL_LIGHTS
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float2 staticLightmapUV  : TEXCOORD1;
                float2 dynamicLightmapUV : TEXCOORD2;
                float3 normalOS    : NORMAL;
                float4 tangentOS   : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 tangentWS   : TEXCOORD3;
                float3 viewDirWS   : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 6);
                #ifdef DYNAMICLIGHTMAP_ON
                    float2 dynamicLightmapUV : TEXCOORD7;
                #endif
                float4 fogFactorAndVertexLight : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            TEXTURE2D(_MaskMap);
            TEXTURE2D(_PhysicsMap);
            TEXTURE2D(_NormalMap);
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
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs vpos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   vnor = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionHCS = vpos.positionCS;
                OUT.positionWS  = vpos.positionWS;
                OUT.normalWS    = vnor.normalWS;
                OUT.tangentWS   = float4(vnor.tangentWS, IN.tangentOS.w);
                OUT.viewDirWS   = GetWorldSpaceViewDir(OUT.positionWS);
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);

                OUT.uv = IN.uv * _Tiling.xy + _Offset.xy;

                // Lightmap UVs and SH
                OUTPUT_LIGHTMAP_UV(IN.staticLightmapUV, unity_LightmapST, OUT.staticLightmapUV);
                #ifdef DYNAMICLIGHTMAP_ON
                    OUT.dynamicLightmapUV = IN.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                OUTPUT_SH(OUT.normalWS.xyz, OUT.vertexSH);

                // Fog and vertex lighting
                #ifdef _ADDITIONAL_LIGHTS_VERTEX
                    half3 vertexLight = VertexLighting(OUT.positionWS, OUT.normalWS);
                #else
                    half3 vertexLight = half3(0, 0, 0);
                #endif
                half fogFactor = ComputeFogFactor(OUT.positionHCS.z);
                OUT.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float2 uv = IN.uv;

                half4 baseColor   = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
                half4 basePhysics = SAMPLE_TEXTURE2D(_PhysicsMap, sampler_BaseMap, uv);
                half3 baseNormal  = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, uv), _NormalStrength);

                half3 cBase         = baseColor.rgb;
                half3 blendedNormal = baseNormal;
                half  roughness     = basePhysics.r;
                half  metallic      = basePhysics.g;
                half  ao            = basePhysics.b;
                half  specular      = _BaseSpecular;

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
                    half w0  = saturate(1.0 - saturate(wR + wG + wB + wA));
                    half sum = max(w0 + wR + wG + wB + wA, 1e-3);
                    w0/=sum; wR/=sum; wG/=sum; wB/=sum; wA/=sum;

                    half3 finalColor = w0 * cBase;
                    blendedNormal = w0 * baseNormal;
                    roughness     = w0 * basePhysics.r;
                    metallic      = w0 * basePhysics.g;
                    ao            = w0 * basePhysics.b;
                    specular      = w0 * _BaseSpecular;

                    #ifdef USE_LAYER_R
                        half3 cR = SAMPLE_TEXTURE2D(_LayerR_Map, sampler_BaseMap, uv).rgb * _LayerRColor.rgb;
                        half3 nR = UnpackNormalScale(SAMPLE_TEXTURE2D(_LayerR_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                        half4 pR = SAMPLE_TEXTURE2D(_LayerR_PhysicsMap, sampler_BaseMap, uv);
                        finalColor    += wR * cR;
                        blendedNormal += wR * nR;
                        roughness     += wR * pR.r;
                        metallic      += wR * pR.g;
                        ao            += wR * pR.b;
                        specular      += wR * _LayerRSpecular;
                    #endif
                    #ifdef USE_LAYER_G
                        half3 cG = SAMPLE_TEXTURE2D(_LayerG_Map, sampler_BaseMap, uv).rgb * _LayerGColor.rgb;
                        half3 nG = UnpackNormalScale(SAMPLE_TEXTURE2D(_LayerG_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                        half4 pG = SAMPLE_TEXTURE2D(_LayerG_PhysicsMap, sampler_BaseMap, uv);
                        finalColor    += wG * cG;
                        blendedNormal += wG * nG;
                        roughness     += wG * pG.r;
                        metallic      += wG * pG.g;
                        ao            += wG * pG.b;
                        specular      += wG * _LayerGSpecular;
                    #endif
                    #ifdef USE_LAYER_B
                        half3 cB = SAMPLE_TEXTURE2D(_LayerB_Map, sampler_BaseMap, uv).rgb * _LayerBColor.rgb;
                        half3 nB = UnpackNormalScale(SAMPLE_TEXTURE2D(_LayerB_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                        half4 pB = SAMPLE_TEXTURE2D(_LayerB_PhysicsMap, sampler_BaseMap, uv);
                        finalColor    += wB * cB;
                        blendedNormal += wB * nB;
                        roughness     += wB * pB.r;
                        metallic      += wB * pB.g;
                        ao            += wB * pB.b;
                        specular      += wB * _LayerBSpecular;
                    #endif
                    #ifdef USE_LAYER_A
                        half3 cA = SAMPLE_TEXTURE2D(_LayerA_Map, sampler_BaseMap, uv).rgb * _LayerAColor.rgb;
                        half3 nA = UnpackNormalScale(SAMPLE_TEXTURE2D(_LayerA_NormalMap, sampler_BaseMap, uv), _NormalStrength);
                        half4 pA = SAMPLE_TEXTURE2D(_LayerA_PhysicsMap, sampler_BaseMap, uv);
                        finalColor    += wA * cA;
                        blendedNormal += wA * nA;
                        roughness     += wA * pA.r;
                        metallic      += wA * pA.g;
                        ao            += wA * pA.b;
                        specular      += wA * _LayerASpecular;
                    #endif
                #else
                    half3 finalColor = cBase;
                #endif

                roughness = saturate(roughness + _RoughAdd);
                metallic  = saturate(metallic  + _MetalAdd);

                blendedNormal = normalize(blendedNormal);

                float3 t = normalize(IN.tangentWS.xyz);
                float3 n = normalize(IN.normalWS);
                float  s = IN.tangentWS.w;
                float3 b = normalize(cross(n, t)) * s;
                float3x3 TBN = float3x3(t, b, n);

                float3 normalWS = normalize(mul(TBN, blendedNormal));

                InputData inputData = (InputData)0;
                inputData.positionWS      = IN.positionWS;
                inputData.normalWS        = normalWS;
                inputData.viewDirectionWS = normalize(IN.viewDirWS);
                inputData.shadowCoord     = IN.shadowCoord;
                inputData.fogCoord        = IN.fogFactorAndVertexLight.x;
                inputData.vertexLighting  = IN.fogFactorAndVertexLight.yzw;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                inputData.shadowMask      = SAMPLE_SHADOWMASK(IN.staticLightmapUV);

                #if defined(DYNAMICLIGHTMAP_ON)
                    inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.dynamicLightmapUV, IN.vertexSH, normalWS);
                #else
                    inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.vertexSH, normalWS);
                #endif

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo     = finalColor;
                surfaceData.metallic   = metallic;
                surfaceData.specular   = half3(specular, specular, specular);
                surfaceData.smoothness = 1 - roughness;
                surfaceData.normalTS   = blendedNormal;
                surfaceData.emission   = 0;
                surfaceData.occlusion  = ao;
                surfaceData.alpha      = baseColor.a;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb   = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
