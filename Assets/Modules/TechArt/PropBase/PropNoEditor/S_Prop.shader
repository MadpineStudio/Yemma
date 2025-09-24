Shader "Custom/teste"
{
    Properties
    {

        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _MaskMap("Mask Map (RGBA)", 2D) = "black" {}
        _PhysicsMap("Physics Map (R=Rough, G=Metal, B=AO)", 2D) = "white" {}
        [Normal] _NormalMap("Normal Map", 2D) = "bump" {}

        _LayerRColor("LayerRColor", Color) = (1, 1, 1, 1)
        _LayerR_Map("Layer R Map", 2D) = "white" {}

        _LayerGColor("LayerGColor", Color) = (1, 1, 1, 1)
        _LayerG_Map("Layer G Map", 2D) = "white" {}

        _LayerBColor("LayerBColor", Color) = (1, 1, 1, 1)
        _LayerB_Map("Layer B Map", 2D) = "white" {}

        _LayerAColor("LayerAColor", Color) = (1, 1, 1, 1)
        _LayerA_Map("Layer A Map", 2D) = "white" {}

        _Tiling("Tiling", Vector) = (1, 1, 0, 0)
        _Offset("Offset", Vector) = (0, 0, 0, 0)

        _NormalStrength("Normal Strength", Range(0, 2)) = 1
        _RoughAdd("Roughness Add", Range(-1, 1)) = 0
        _MetalAdd("Metallic Add", Range(-1, 1)) = 0

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
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _MIXED_LIGHTING
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _MASKMAP
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            TEXTURE2D(_MaskMap);
            TEXTURE2D(_PhysicsMap);
            TEXTURE2D(_NormalMap);
            TEXTURE2D(_LayerR_Map);
            TEXTURE2D(_LayerG_Map);
            TEXTURE2D(_LayerB_Map);
            TEXTURE2D(_LayerA_Map);
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
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                
                OUT.uv = IN.uv * _Tiling.xy + _Offset.xy;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                
                // Sample base and mask
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
                half4 physics = SAMPLE_TEXTURE2D(_PhysicsMap, sampler_BaseMap, uv);
                
                // Normalized layer blending
                half4 m = SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, uv);
                half wR = m.r, wG = m.g, wB = m.b, wA = m.a;
                half w0 = saturate(1.0 - saturate(wR + wG + wB + wA));
                half sum = max(w0 + wR + wG + wB + wA, 1e-3);
                w0 /= sum; wR /= sum; wG /= sum; wB /= sum; wA /= sum;
                
                half3 cBase = baseColor.rgb;
                half3 cR = SAMPLE_TEXTURE2D(_LayerR_Map, sampler_BaseMap, uv).rgb * _LayerRColor.rgb;
                half3 cG = SAMPLE_TEXTURE2D(_LayerG_Map, sampler_BaseMap, uv).rgb * _LayerGColor.rgb;
                half3 cB = SAMPLE_TEXTURE2D(_LayerB_Map, sampler_BaseMap, uv).rgb * _LayerBColor.rgb;
                half3 cA = SAMPLE_TEXTURE2D(_LayerA_Map, sampler_BaseMap, uv).rgb * _LayerAColor.rgb;
                
                half3 finalColor = w0*cBase + wR*cR + wG*cG + wB*cB + wA*cA;
                
                // Reconstruct TBN and sample normal
                half3 t = normalize(IN.tangentWS);
                half3 n = normalize(IN.normalWS);
                half3 b = normalize(cross(n, t));
                half3x3 TBN = half3x3(t, b, n);
                half3 normalWS = normalize(mul(UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, uv), _NormalStrength), TBN));
                
                // Physics properties with layer modulation
                half layerWeight = wR + wG + wB + wA;
                half roughness = saturate(physics.r + layerWeight * _RoughAdd);
                half metallic = saturate(physics.g + layerWeight * _MetalAdd);
                half ao = physics.b;
                
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = normalize(GetCameraPositionWS() - IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.bakedGI = SAMPLE_GI(IN.uv, normalWS, inputData.viewDirectionWS);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalColor;
                surfaceData.metallic = metallic;
                surfaceData.specular = half3(0, 0, 0);
                surfaceData.smoothness = 1 - roughness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.emission = half3(0, 0, 0);
                surfaceData.occlusion = ao;
                surfaceData.alpha = baseColor.a;
                
                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
    }
}