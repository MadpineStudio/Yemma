// PROPBASE SHADER WITH MATCAP SUPPORT - TRIMSHEET OPTIMIZED
// - 4-layer system with RGBA masking
// - Each layer can use either PBR mode OR MatCap mode (exclusive)
// - MatCap: Camera-based sphere mapping with rotation, intensity, contrast, saturation
// - PBR: Full metallic workflow with normal mapping, emission, UV transforms
// - Optimized for trimsheets - no base textures needed
// - Optimized for Nintendo Switch with minimal lighting
Shader "Custom/S_PropBase"
{
    Properties
    {
        [Header(Base Layer)]
        _BaseTexture("Base Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        _BaseUV("Base UV", Vector) = (1, 1, 0, 0)
        
        [Header(Layer Masks)]
        [Toggle] _UseLayerMask("Use Layer Mask", Float) = 0
        _LayerMask("Layer Mask (RGBA)", 2D) = "white" {}
        
        [Header(Layer 01  R Channel)]
        [Toggle] _UseLayer01("Use Layer 01", Float) = 1
        _Layer01Texture("Layer 01 Texture", 2D) = "white" {}
        _Layer01Color("Layer 01 Color", Color) = (1, 1, 1, 1)
        _Layer01Intensity("Layer 01 Intensity", Range(0, 2)) = 1
        _Layer01UV("Layer 01 UV", Vector) = (1, 1, 0, 0)
        [Toggle] _Layer01UsePBR("Layer 01 Use PBR", Float) = 1
        _Layer01Normal("Layer 01 Normal", 2D) = "bump" {}
        _Layer01NormalScale("Layer 01 Normal Scale", Range(0, 2)) = 1
        _Layer01Metallic("Layer 01 Metallic", Range(0, 1)) = 0
        _Layer01Smoothness("Layer 01 Smoothness", Range(0, 1)) = 0.5
        _Layer01Emission("Layer 01 Emission", Color) = (0, 0, 0, 0)
        _Layer01EmissionIntensity("Layer 01 Emission Intensity", Range(0, 10)) = 1
        [Toggle] _Layer01UseMatCap("Layer 01 Use MatCap", Float) = 0
        _Layer01MatCapTexture("Layer 01 MatCap Texture", 2D) = "white" {}
        _Layer01MatCapIntensity("Layer 01 MatCap Intensity", Range(0, 3)) = 1
        _Layer01MatCapContrast("Layer 01 MatCap Contrast", Range(0.1, 2)) = 1
        _Layer01MatCapSaturation("Layer 01 MatCap Saturation", Range(0, 2)) = 1
        _Layer01MatCapRotation("Layer 01 MatCap Rotation", Range(0, 360)) = 0
        
        [Header(Layer 02  G Channel)]
        [Toggle] _UseLayer02("Use Layer 02", Float) = 0
        _Layer02Texture("Layer 02 Texture", 2D) = "white" {}
        _Layer02Color("Layer 02 Color", Color) = (1, 1, 1, 1)
        _Layer02Intensity("Layer 02 Intensity", Range(0, 2)) = 1
        _Layer02UV("Layer 02 UV", Vector) = (1, 1, 0, 0)
        [Toggle] _Layer02UsePBR("Layer 02 Use PBR", Float) = 1
        _Layer02Normal("Layer 02 Normal", 2D) = "bump" {}
        _Layer02NormalScale("Layer 02 Normal Scale", Range(0, 2)) = 1
        _Layer02Metallic("Layer 02 Metallic", Range(0, 1)) = 0
        _Layer02Smoothness("Layer 02 Smoothness", Range(0, 1)) = 0.5
        _Layer02Emission("Layer 02 Emission", Color) = (0, 0, 0, 0)
        _Layer02EmissionIntensity("Layer 02 Emission Intensity", Range(0, 10)) = 1
        [Toggle] _Layer02UseMatCap("Layer 02 Use MatCap", Float) = 0
        _Layer02MatCapTexture("Layer 02 MatCap Texture", 2D) = "white" {}
        _Layer02MatCapIntensity("Layer 02 MatCap Intensity", Range(0, 3)) = 1
        _Layer02MatCapContrast("Layer 02 MatCap Contrast", Range(0.1, 2)) = 1
        _Layer02MatCapSaturation("Layer 02 MatCap Saturation", Range(0, 2)) = 1
        _Layer02MatCapRotation("Layer 02 MatCap Rotation", Range(0, 360)) = 0
        
        [Header(Layer 03  B Channel)]
        [Toggle] _UseLayer03("Use Layer 03", Float) = 0
        _Layer03Texture("Layer 03 Texture", 2D) = "white" {}
        _Layer03Color("Layer 03 Color", Color) = (1, 1, 1, 1)
        _Layer03Intensity("Layer 03 Intensity", Range(0, 2)) = 1
        _Layer03UV("Layer 03 UV", Vector) = (1, 1, 0, 0)
        [Toggle] _Layer03UsePBR("Layer 03 Use PBR", Float) = 1
        _Layer03Normal("Layer 03 Normal", 2D) = "bump" {}
        _Layer03NormalScale("Layer 03 Normal Scale", Range(0, 2)) = 1
        _Layer03Metallic("Layer 03 Metallic", Range(0, 1)) = 0
        _Layer03Smoothness("Layer 03 Smoothness", Range(0, 1)) = 0.5
        _Layer03Emission("Layer 03 Emission", Color) = (0, 0, 0, 0)
        _Layer03EmissionIntensity("Layer 03 Emission Intensity", Range(0, 10)) = 1
        [Toggle] _Layer03UseMatCap("Layer 03 Use MatCap", Float) = 0
        _Layer03MatCapTexture("Layer 03 MatCap Texture", 2D) = "white" {}
        _Layer03MatCapIntensity("Layer 03 MatCap Intensity", Range(0, 3)) = 1
        _Layer03MatCapContrast("Layer 03 MatCap Contrast", Range(0.1, 2)) = 1
        _Layer03MatCapSaturation("Layer 03 MatCap Saturation", Range(0, 2)) = 1
        _Layer03MatCapRotation("Layer 03 MatCap Rotation", Range(0, 360)) = 0
        
        [Header(Layer 04  A Channel)]
        [Toggle] _UseLayer04("Use Layer 04", Float) = 0
        _Layer04Texture("Layer 04 Texture", 2D) = "white" {}
        _Layer04Color("Layer 04 Color", Color) = (1, 1, 1, 1)
        _Layer04Intensity("Layer 04 Intensity", Range(0, 2)) = 1
        _Layer04UV("Layer 04 UV", Vector) = (1, 1, 0, 0)
        [Toggle] _Layer04UsePBR("Layer 04 Use PBR", Float) = 1
        _Layer04Normal("Layer 04 Normal", 2D) = "bump" {}
        _Layer04NormalScale("Layer 04 Normal Scale", Range(0, 2)) = 1
        _Layer04Metallic("Layer 04 Metallic", Range(0, 1)) = 0
        _Layer04Smoothness("Layer 04 Smoothness", Range(0, 1)) = 0.5
        _Layer04Emission("Layer 04 Emission", Color) = (0, 0, 0, 0)
        _Layer04EmissionIntensity("Layer 04 Emission Intensity", Range(0, 10)) = 1
        [Toggle] _Layer04UseMatCap("Layer 04 Use MatCap", Float) = 0
        _Layer04MatCapTexture("Layer 04 MatCap Texture", 2D) = "white" {}
        _Layer04MatCapIntensity("Layer 04 MatCap Intensity", Range(0, 3)) = 1
        _Layer04MatCapContrast("Layer 04 MatCap Contrast", Range(0.1, 2)) = 1
        _Layer04MatCapSaturation("Layer 04 MatCap Saturation", Range(0, 2)) = 1
        _Layer04MatCapRotation("Layer 04 MatCap Rotation", Range(0, 360)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // Force additional lights support
            #if !defined(_ADDITIONAL_LIGHTS) && !defined(_ADDITIONAL_LIGHTS_VERTEX)
                #define _ADDITIONAL_LIGHTS
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 staticLightmapUV : TEXCOORD1;
                float2 dynamicLightmapUV : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 6);
                #ifdef DYNAMICLIGHTMAP_ON
                float2 dynamicLightmapUV : TEXCOORD7;
                #endif
                float4 fogFactorAndVertexLight : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseTexture);
            TEXTURE2D(_LayerMask);
            
            TEXTURE2D(_Layer01Texture);
            TEXTURE2D(_Layer01Normal);
            TEXTURE2D(_Layer01MatCapTexture);
            
            TEXTURE2D(_Layer02Texture);
            TEXTURE2D(_Layer02Normal);
            TEXTURE2D(_Layer02MatCapTexture);
            
            TEXTURE2D(_Layer03Texture);
            TEXTURE2D(_Layer03Normal);
            TEXTURE2D(_Layer03MatCapTexture);
            
            TEXTURE2D(_Layer04Texture);
            TEXTURE2D(_Layer04Normal);
            TEXTURE2D(_Layer04MatCapTexture);
            
            // Shared sampler for all textures
            // SAMPLER(sampler_LinearRepeat);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseTexture_ST;
                float4 _BaseUV;
                
                float4 _LayerMask_ST;
                float4 _Layer01Texture_ST;
                float4 _Layer02Texture_ST;
                float4 _Layer03Texture_ST;
                float4 _Layer04Texture_ST;
                
                float _UseLayerMask;
                
                float _UseLayer01;
                half4 _Layer01Color;
                float _Layer01Intensity;
                float4 _Layer01UV;
                float _Layer01UsePBR;
                float _Layer01Metallic;
                float _Layer01Smoothness;
                float _Layer01NormalScale;
                half4 _Layer01Emission;
                float _Layer01EmissionIntensity;
                float _Layer01UseMatCap;
                float _Layer01MatCapIntensity;
                float _Layer01MatCapContrast;
                float _Layer01MatCapSaturation;
                float _Layer01MatCapRotation;
                
                float _UseLayer02;
                half4 _Layer02Color;
                float _Layer02Intensity;
                float4 _Layer02UV;
                float _Layer02UsePBR;
                float _Layer02Metallic;
                float _Layer02Smoothness;
                float _Layer02NormalScale;
                half4 _Layer02Emission;
                float _Layer02EmissionIntensity;
                float _Layer02UseMatCap;
                float _Layer02MatCapIntensity;
                float _Layer02MatCapContrast;
                float _Layer02MatCapSaturation;
                float _Layer02MatCapRotation;
                
                float _UseLayer03;
                half4 _Layer03Color;
                float _Layer03Intensity;
                float4 _Layer03UV;
                float _Layer03UsePBR;
                float _Layer03Metallic;
                float _Layer03Smoothness;
                float _Layer03NormalScale;
                half4 _Layer03Emission;
                float _Layer03EmissionIntensity;
                float _Layer03UseMatCap;
                float _Layer03MatCapIntensity;
                float _Layer03MatCapContrast;
                float _Layer03MatCapSaturation;
                float _Layer03MatCapRotation;
                
                float _UseLayer04;
                half4 _Layer04Color;
                float _Layer04Intensity;
                float4 _Layer04UV;
                float _Layer04UsePBR;
                float _Layer04Metallic;
                float _Layer04Smoothness;
                float _Layer04NormalScale;
                half4 _Layer04Emission;
                float _Layer04EmissionIntensity;
                float _Layer04UseMatCap;
                float _Layer04MatCapIntensity;
                float _Layer04MatCapContrast;
                float _Layer04MatCapSaturation;
                float _Layer04MatCapRotation;
            CBUFFER_END

            // MatCap functions from S_MatCapGold
            float2 RotateUV(float2 uv, float rotation)
            {
                float rad = radians(rotation);
                float cosRot = cos(rad);
                float sinRot = sin(rad);
                uv -= 0.5;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosRot - uv.y * sinRot;
                rotatedUV.y = uv.x * sinRot + uv.y * cosRot;
                return rotatedUV + 0.5;
            }

            float3 AdjustMatCapColor(float3 color, float intensity, float contrast, float saturation)
            {
                color *= intensity;
                color = pow(color, contrast);
                float gray = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(gray, color, saturation);
            }

            float2 CalculateMatCapUV(float3 normalWS, float3 viewDirWS)
            {
                float3 normalVS = TransformWorldToViewDir(normalWS, true);
                float3 viewDirVS = TransformWorldToViewDir(viewDirWS, true);
                float3 reflectionVS = reflect(-viewDirVS, normalVS);
                float2 matCapUV = reflectionVS.xy * 0.5 + 0.5;
                return matCapUV;
            }

            struct LayerData
            {
                half3 albedo;
                half3 emission;
                float metallic;
                float smoothness;
                half3 normal;
                bool useMatCap;
                half3 matCapColor;
            };

            LayerData ApplyLayer(LayerData baseData, LayerData layerData, float mask, float intensity)
            {
                LayerData result;
                
                // If mask is very low, keep base data unchanged
                if (mask < 0.001)
                {
                    return baseData;
                }
                
                // If layer uses MatCap, completely replace where mask exists
                if (layerData.useMatCap)
                {
                    // MatCap completely replaces base data where mask exists
                    result.albedo = lerp(baseData.albedo, layerData.matCapColor * intensity, mask);
                    result.emission = lerp(baseData.emission, half3(0, 0, 0), mask); // MatCap overrides emission
                    result.metallic = lerp(baseData.metallic, 0, mask); // MatCap overrides metallic
                    result.smoothness = lerp(baseData.smoothness, 0.5, mask); // MatCap overrides smoothness
                    result.normal = lerp(baseData.normal, half3(0, 0, 1), mask); // MatCap overrides normal
                }
                else
                {
                    // Normal PBR mode - layers override base data where mask exists
                    result.albedo = lerp(baseData.albedo, layerData.albedo * intensity, mask);
                    result.emission = lerp(baseData.emission, layerData.emission, mask);
                    result.metallic = lerp(baseData.metallic, layerData.metallic, mask);
                    result.smoothness = lerp(baseData.smoothness, layerData.smoothness, mask);
                    result.normal = normalize(lerp(baseData.normal, layerData.normal, mask));
                }
                
                result.useMatCap = false; // Result should not carry MatCap flag
                result.matCapColor = half3(0, 0, 0); // Clear MatCap color
                
                return result;
            }

            float2 ApplyUVTransform(float2 uv, float4 uvTransform)
            {
                return uv * uvTransform.xy + uvTransform.zw;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
                OUT.viewDirWS = GetWorldSpaceViewDir(OUT.positionWS);
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);
                
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
                
                // Initialize with base texture and color
                float2 baseUV = ApplyUVTransform(IN.uv, _BaseUV);
                half3 baseTexture = SAMPLE_TEXTURE2D(_BaseTexture, sampler_LinearRepeat, baseUV).rgb;
                
                LayerData finalData;
                finalData.albedo = baseTexture * _BaseColor.rgb;
                finalData.emission = half3(0, 0, 0);
                finalData.metallic = 0;
                finalData.smoothness = 0.5;
                finalData.normal = half3(0, 0, 1);
                finalData.useMatCap = false;
                finalData.matCapColor = half3(0, 0, 0);
                
                // Layer masks
                half4 layerMasks = half4(1, 1, 1, 1);
                if (_UseLayerMask > 0.5)
                {
                    layerMasks = SAMPLE_TEXTURE2D(_LayerMask, sampler_LinearRepeat, TRANSFORM_TEX(IN.uv, _LayerMask));
                }
                
                // Compute exclusive masks to enforce strict priority: 04 > 03 > 02 > 01
                // Gate masks by layer enable flags so inactive higher layers don't block lower ones
                half4 rm = layerMasks;
                half m1 = (_UseLayer01 > 0.5) ? saturate(rm.r) : 0.0h;
                half m2 = (_UseLayer02 > 0.5) ? saturate(rm.g) : 0.0h;
                half m3 = (_UseLayer03 > 0.5) ? saturate(rm.b) : 0.0h;
                half m4 = (_UseLayer04 > 0.5) ? saturate(rm.a) : 0.0h;
                
                // Residual method ensures strict override and sum <= 1
                half rem = 1.0h;
                half e4 = min(m4, rem);
                rem = max(0.0h, rem - e4);
                half e3 = min(m3, rem);
                rem = max(0.0h, rem - e3);
                half e2 = min(m2, rem);
                rem = max(0.0h, rem - e2);
                half e1 = min(m1, rem);
                
                // LAYER PRIORITY: Layer 04 > Layer 03 > Layer 02 > Layer 01
                // Higher numbered layers ALWAYS override lower numbered layers
                // MatCap colors should appear without contamination from lower layers
                
                // Layer 01 - R Channel (LOWEST PRIORITY)
                if (_UseLayer01 > 0.5)
                {
                    float2 layer01UV = ApplyUVTransform(IN.uv, _Layer01UV);
                    LayerData layer01Data;
                    
                    // Check if using MatCap or normal PBR
                    if (_Layer01UseMatCap > 0.5)
                    {
                        // MatCap mode
                        float2 matCapUV = CalculateMatCapUV(IN.normalWS, IN.viewDirWS);
                        matCapUV = RotateUV(matCapUV, _Layer01MatCapRotation);
                        half3 matCapColor = SAMPLE_TEXTURE2D(_Layer01MatCapTexture, sampler_LinearRepeat, matCapUV).rgb;
                        matCapColor = AdjustMatCapColor(matCapColor, _Layer01MatCapIntensity, _Layer01MatCapContrast, _Layer01MatCapSaturation);
                        
                        layer01Data.useMatCap = true;
                        layer01Data.matCapColor = matCapColor * _Layer01Color.rgb;
                        layer01Data.albedo = half3(0, 0, 0); // Not used in MatCap mode
                        layer01Data.emission = half3(0, 0, 0);
                        layer01Data.metallic = 0;
                        layer01Data.smoothness = 0;
                        layer01Data.normal = half3(0, 0, 1);
                    }
                    else
                    {
                        // Normal PBR mode
                        layer01Data.useMatCap = false;
                        layer01Data.matCapColor = half3(0, 0, 0);
                        layer01Data.albedo = SAMPLE_TEXTURE2D(_Layer01Texture, sampler_LinearRepeat, layer01UV).rgb * _Layer01Color.rgb;
                        
                        // Use PBR parameters only if enabled
                        if (_Layer01UsePBR > 0.5)
                        {
                            layer01Data.emission = _Layer01Emission.rgb * _Layer01EmissionIntensity;
                            layer01Data.metallic = _Layer01Metallic;
                            layer01Data.smoothness = _Layer01Smoothness;
                            layer01Data.normal = UnpackNormalScale(SAMPLE_TEXTURE2D(_Layer01Normal, sampler_LinearRepeat, layer01UV), _Layer01NormalScale);
                        }
                        else
                        {
                            // Skip PBR calculations for better performance
                            layer01Data.emission = half3(0, 0, 0);
                            layer01Data.metallic = 0;
                            layer01Data.smoothness = 0.5; // Default smoothness
                            layer01Data.normal = half3(0, 0, 1); // Default normal
                        }
                    }
                    
                    finalData = ApplyLayer(finalData, layer01Data, e1, _Layer01Intensity);
                }
                
                // Layer 02 - G Channel (HIGHER PRIORITY than Layer 01)
                if (_UseLayer02 > 0.5)
                {
                    float2 layer02UV = ApplyUVTransform(IN.uv, _Layer02UV);
                    LayerData layer02Data;
                    
                    // Check if using MatCap or normal PBR
                    if (_Layer02UseMatCap > 0.5)
                    {
                        // MatCap mode
                        float2 matCapUV = CalculateMatCapUV(IN.normalWS, IN.viewDirWS);
                        matCapUV = RotateUV(matCapUV, _Layer02MatCapRotation);
                        half3 matCapColor = SAMPLE_TEXTURE2D(_Layer02MatCapTexture, sampler_LinearRepeat, matCapUV).rgb;
                        matCapColor = AdjustMatCapColor(matCapColor, _Layer02MatCapIntensity, _Layer02MatCapContrast, _Layer02MatCapSaturation);
                        
                        layer02Data.useMatCap = true;
                        layer02Data.matCapColor = matCapColor * _Layer02Color.rgb;
                        layer02Data.albedo = half3(0, 0, 0);
                        layer02Data.emission = half3(0, 0, 0);
                        layer02Data.metallic = 0;
                        layer02Data.smoothness = 0;
                        layer02Data.normal = half3(0, 0, 1);
                    }
                    else
                    {
                        // Normal PBR mode
                        layer02Data.useMatCap = false;
                        layer02Data.matCapColor = half3(0, 0, 0);
                        layer02Data.albedo = SAMPLE_TEXTURE2D(_Layer02Texture, sampler_LinearRepeat, layer02UV).rgb * _Layer02Color.rgb;
                        
                        // Use PBR parameters only if enabled
                        if (_Layer02UsePBR > 0.5)
                        {
                            layer02Data.emission = _Layer02Emission.rgb * _Layer02EmissionIntensity;
                            layer02Data.metallic = _Layer02Metallic;
                            layer02Data.smoothness = _Layer02Smoothness;
                            layer02Data.normal = UnpackNormalScale(SAMPLE_TEXTURE2D(_Layer02Normal, sampler_LinearRepeat, layer02UV), _Layer02NormalScale);
                        }
                        else
                        {
                            // Skip PBR calculations for better performance
                            layer02Data.emission = half3(0, 0, 0);
                            layer02Data.metallic = 0;
                            layer02Data.smoothness = 0.5; // Default smoothness
                            layer02Data.normal = half3(0, 0, 1); // Default normal
                        }
                    }
                    
                    finalData = ApplyLayer(finalData, layer02Data, e2, _Layer02Intensity);
                }
                
                // Layer 03 - B Channel (HIGHER PRIORITY than Layer 01-02)
                if (_UseLayer03 > 0.5)
                {
                    float2 layer03UV = ApplyUVTransform(IN.uv, _Layer03UV);
                    LayerData layer03Data;
                    
                    // Check if using MatCap or normal PBR
                    if (_Layer03UseMatCap > 0.5)
                    {
                        // MatCap mode
                        float2 matCapUV = CalculateMatCapUV(IN.normalWS, IN.viewDirWS);
                        matCapUV = RotateUV(matCapUV, _Layer03MatCapRotation);
                        half3 matCapColor = SAMPLE_TEXTURE2D(_Layer03MatCapTexture, sampler_LinearRepeat, matCapUV).rgb;
                        matCapColor = AdjustMatCapColor(matCapColor, _Layer03MatCapIntensity, _Layer03MatCapContrast, _Layer03MatCapSaturation);
                        
                        layer03Data.useMatCap = true;
                        layer03Data.matCapColor = matCapColor * _Layer03Color.rgb;
                        layer03Data.albedo = half3(0, 0, 0);
                        layer03Data.emission = half3(0, 0, 0);
                        layer03Data.metallic = 0;
                        layer03Data.smoothness = 0;
                        layer03Data.normal = half3(0, 0, 1);
                    }
                    else
                    {
                        // Normal PBR mode
                        layer03Data.useMatCap = false;
                        layer03Data.matCapColor = half3(0, 0, 0);
                        layer03Data.albedo = SAMPLE_TEXTURE2D(_Layer03Texture, sampler_LinearRepeat, layer03UV).rgb * _Layer03Color.rgb;
                        
                        // Use PBR parameters only if enabled
                        if (_Layer03UsePBR > 0.5)
                        {
                            layer03Data.emission = _Layer03Emission.rgb * _Layer03EmissionIntensity;
                            layer03Data.metallic = _Layer03Metallic;
                            layer03Data.smoothness = _Layer03Smoothness;
                            layer03Data.normal = UnpackNormalScale(SAMPLE_TEXTURE2D(_Layer03Normal, sampler_LinearRepeat, layer03UV), _Layer03NormalScale);
                        }
                        else
                        {
                            // Skip PBR calculations for better performance
                            layer03Data.emission = half3(0, 0, 0);
                            layer03Data.metallic = 0;
                            layer03Data.smoothness = 0.5; // Default smoothness
                            layer03Data.normal = half3(0, 0, 1); // Default normal
                        }
                    }
                    
                    finalData = ApplyLayer(finalData, layer03Data, e3, _Layer03Intensity);
                }
                
                // Layer 04 - A Channel (HIGHEST PRIORITY - overrides all other layers)
                if (_UseLayer04 > 0.5)
                {
                    float2 layer04UV = ApplyUVTransform(IN.uv, _Layer04UV);
                    LayerData layer04Data;
                    
                    // Check if using MatCap or normal PBR
                    if (_Layer04UseMatCap > 0.5)
                    {
                        // MatCap mode
                        float2 matCapUV = CalculateMatCapUV(IN.normalWS, IN.viewDirWS);
                        matCapUV = RotateUV(matCapUV, _Layer04MatCapRotation);
                        half3 matCapColor = SAMPLE_TEXTURE2D(_Layer04MatCapTexture, sampler_LinearRepeat, matCapUV).rgb;
                        matCapColor = AdjustMatCapColor(matCapColor, _Layer04MatCapIntensity, _Layer04MatCapContrast, _Layer04MatCapSaturation);
                        
                        layer04Data.useMatCap = true;
                        layer04Data.matCapColor = matCapColor * _Layer04Color.rgb;
                        layer04Data.albedo = half3(0, 0, 0);
                        layer04Data.emission = half3(0, 0, 0);
                        layer04Data.metallic = 0;
                        layer04Data.smoothness = 0;
                        layer04Data.normal = half3(0, 0, 1);
                    }
                    else
                    {
                        // Normal PBR mode
                        layer04Data.useMatCap = false;
                        layer04Data.matCapColor = half3(0, 0, 0);
                        layer04Data.albedo = SAMPLE_TEXTURE2D(_Layer04Texture, sampler_LinearRepeat, layer04UV).rgb * _Layer04Color.rgb;
                        
                        // Use PBR parameters only if enabled
                        if (_Layer04UsePBR > 0.5)
                        {
                            layer04Data.emission = _Layer04Emission.rgb * _Layer04EmissionIntensity;
                            layer04Data.metallic = _Layer04Metallic;
                            layer04Data.smoothness = _Layer04Smoothness;
                            layer04Data.normal = UnpackNormalScale(SAMPLE_TEXTURE2D(_Layer04Normal, sampler_LinearRepeat, layer04UV), _Layer04NormalScale);
                        }
                        else
                        {
                            // Skip PBR calculations for better performance
                            layer04Data.emission = half3(0, 0, 0);
                            layer04Data.metallic = 0;
                            layer04Data.smoothness = 0.5; // Default smoothness
                            layer04Data.normal = half3(0, 0, 1); // Default normal
                        }
                    }
                    
                    finalData = ApplyLayer(finalData, layer04Data, e4, _Layer04Intensity);
                }
                
                // Setup surface data for PBR lighting
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalData.albedo;
                surfaceData.metallic = finalData.metallic;
                surfaceData.specular = half3(0, 0, 0);
                surfaceData.smoothness = finalData.smoothness;
                surfaceData.normalTS = finalData.normal;
                surfaceData.emission = finalData.emission;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1.0; // Opaque for trimsheets
                surfaceData.clearCoatMask = 0.0;
                surfaceData.clearCoatSmoothness = 0.0;
                
                // Setup input data for lighting
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = TransformTangentToWorld(finalData.normal, half3x3(IN.tangentWS.xyz, cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w, IN.normalWS));
                inputData.normalWS = normalize(inputData.normalWS);
                inputData.viewDirectionWS = normalize(IN.viewDirWS);
                inputData.shadowCoord = IN.shadowCoord;
                inputData.fogCoord = IN.fogFactorAndVertexLight.x;
                inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(IN.staticLightmapUV);
                
                // Sample lightmaps and light probes
                #if defined(DYNAMICLIGHTMAP_ON)
                inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.dynamicLightmapUV, IN.vertexSH, inputData.normalWS);
                #else
                inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.vertexSH, inputData.normalWS);
                #endif
                
                // Calculate PBR lighting
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // Apply fog
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                
                return color;
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
            #pragma target 2.0

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
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
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }

            ENDHLSL
        }
    }
    
    CustomEditor "PropBase.Editor.PropBaseShaderEditor"
}
