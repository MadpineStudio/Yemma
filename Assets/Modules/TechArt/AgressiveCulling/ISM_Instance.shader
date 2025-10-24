Shader "Custom/ISM_Instance"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct InstanceData
            {
                float4x4 matrix;
                float4 customData;
            };

            StructuredBuffer<uint> _VisibleIDBuffer;
            StructuredBuffer<InstanceData> _InstanceDataBuffer;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Glossiness;
                float _Metallic;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 customData : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                uint instanceID = _VisibleIDBuffer[input.instanceID];
                InstanceData data = _InstanceDataBuffer[instanceID];

                float4 positionWS = mul(data.matrix, input.positionOS);
                output.positionCS = mul(UNITY_MATRIX_VP, positionWS);
                output.positionWS = positionWS.xyz;
                output.normalWS = mul((float3x3)data.matrix, input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.customData = data.customData;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                
                float3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                
                float3 lighting = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                
                half3 color = albedo.rgb * lighting * NdotL;
                color += albedo.rgb * 0.2; // Ambient

                return half4(color, albedo.a);
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
            #pragma vertex vertShadow
            #pragma fragment fragShadow

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct InstanceData
            {
                float4x4 matrix;
                float4 customData;
            };

            StructuredBuffer<uint> _VisibleIDBuffer;
            StructuredBuffer<InstanceData> _InstanceDataBuffer;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vertShadow(Attributes input)
            {
                Varyings output;
                uint instanceID = _VisibleIDBuffer[input.instanceID];
                InstanceData data = _InstanceDataBuffer[instanceID];

                float4 positionWS = mul(data.matrix, input.positionOS);
                output.positionCS = mul(UNITY_MATRIX_VP, positionWS);
                return output;
            }

            half4 fragShadow(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
