Shader "Custom/OptimizedGrassRendererURP"
{
    Properties
    {
        _WindScale("Wind Scale", Range(.0001, 3)) = 0.1
        _WindStrength("Wind Strength", Float) = 0.1
        _WindVelocity("Wind Velocity", Vector) = (0,0,0,0)
        _WindTex("Wind Texture", 2D) = "white" {}
        _WindColor("Wind Color", Color) = (0,0,0,0)
        _Color("Base Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (0.5,0.5,0.5,1)
        _LightColor("Light Color", Color) = (0,0,0,0)
        _GrassScale("Grass Scale", Float) = 1.0
        _Intensity("Color Intensity", Float) = 0.1
        _BaseOffsetY("Base Offset Y", Float) = 0.1
        _GrassMask("Grass Mask", 2D) = "white" {}
        _GrassGroundMask("Ground Mask", 2D) = "white" {}
        _InputMask("Input Mask", 2D) = "white" {}
        _shadowOcclusion("Shadow Occlusion", Float) = 1.0
        _shadowOcclusionHeight("Shadow Occlusion Height", Float) = 0.0
        _grassDisplaceScale("Displace Scale", Float) = 0.1
        _grassDisplaceIntensity("Displace Intensity", Float) = 0.1
        _grassOffset("Displace Offset", Vector) = (0,0,0,0)
        _Test("Teste", Range(-.1,.1)) = 0
        _Edge0("_Edge0", Range(-.1,.1)) = 0
        _Edge1("_Edge1", Range(-.1,.1)) = 0
        _Alpha("_Alpha", Range(-.1,.1)) = 0
    }

    SubShader
    {
        Tags { 
            "RenderType" = "TransparentCutout"
            "Queue" = "Transparent"
//            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        
        Cull Off
        LOD 0
        ZWrite On
        ZTest On
        ZClip True

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
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct DrawVertex {
                float3 positionWS;
                float2 uv;
            };

            struct DrawTriangle {
                float3 normalWS;
                DrawVertex vertices[3];
            };

            struct Attributes {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float windStrength  : TEXCOORD2;
                float3 positionWS   : TEXCOORD3;
                float4 shadowCoord  : TEXCOORD4;
                float3 displace     : TEXCOORD5;
                float fogCoord      : TEXCOORD6;
                float2 uvRand4x1    : TEXCOORD7;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _WindTex;
            sampler2D _GrassMask;
            sampler2D _GrassGroundMask;
            sampler2D _InputMask;

            CBUFFER_START(UnityPerMaterial)
            float4 _WindTex_ST;
            float4 _GrassMask_ST;
            float4 _GrassGroundMask_ST;
            float4 _InputMask_ST;
            float2 _WindVelocity;
            float _WindStrength;
            float _WindScale;
            float _GrassScale;
            float _BaseOffsetY;
            float _Intensity;
            float _shadowOcclusion;
            float _shadowOcclusionHeight;
            float _grassDisplaceScale;
            float _grassDisplaceIntensity;
            float _Test;
            float2 _grassOffset;
            half3 _ShadowColor;
            half3 _Color;
            half3 _LightColor;
            half3 _WindColor;
            float _Edge0;
            float _Edge1;
            float _Alpha;
            CBUFFER_END

            StructuredBuffer<DrawTriangle> _DrawTriangles;
            StructuredBuffer<float2> _Grass_UVTex; 

            float3 FixBackfaceNormal(float3 normalWS, float3 viewDirWS)
            {
                return dot(normalWS, viewDirWS) < 0 ? -normalWS : normalWS;
            }

            float CalculateWind(float2 positionXZ, float uvY)
            {
                float2 windUV = positionXZ * _WindScale + _WindVelocity * _Time.y;
                float windSample = tex2Dlod(_WindTex, float4(windUV, 0, 0)).r;
                return pow(windSample, _WindStrength) * uvY;
            }

            float3 CalculateDisplacement(float2 positionXZ)
            {
                float2 displaceUV = positionXZ * _grassDisplaceScale + _grassOffset;
                return tex2Dlod(_InputMask, float4(displaceUV, 0, 0)).rgb;
            }

            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                int triIndex = input.vertexID / 3;
                int vertIndex = input.vertexID % 3;
                
                DrawTriangle tri = _DrawTriangles[triIndex];
                DrawVertex v = tri.vertices[vertIndex];

                int quad = triIndex / 2;
                int triInQuad = triIndex % 2;
                
                output.uv = v.uv;
                output.positionWS = v.positionWS;

                // TODO: ACESSAR ESSE BUFFER DE FORMA CORRETA PARA DESENHAR NOS QUADS A UV CORRETA QUE VEM DO COMPUTE
                output.uvRand4x1 = _Grass_UVTex[triInQuad];

                float3 viewDirWS = GetWorldSpaceViewDir(v.positionWS);
                output.normalWS = FixBackfaceNormal(tri.normalWS, viewDirWS);
                output.windStrength = CalculateWind(v.positionWS.xz, v.uv.y);
                // output.positionWS.xz += output.windStrength;
                output.displace = 0;
                // output.displace = CalculateDisplacement(v.positionWS.xz);
                
                // output.positionWS.xz += output.displace.xy * _grassDisplaceIntensity;
                // output.positionWS.y -= output.displace.z * _grassDisplaceIntensity;
                
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }
            float MyLuminance(float3 color)
            {
                return dot(color, float3(0.2126, 0.7152, 0.0722));
            }
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half3 grassMask = tex2D(_GrassMask, input.uv - float2(0.05, 0.1)).rgb;
                // clip(grassMask.r - 0.1);

                float3 groundColor = tex2D(_GrassGroundMask, input.positionWS.xz * _GrassScale).rgb;
                Light mainLight = GetMainLight(input.shadowCoord);
                float shadowAtten = smoothstep(0.25, 0.75, mainLight.shadowAttenuation);
                float NdotL = saturate(dot(input.normalWS, mainLight.direction));
                float occlusion = saturate(lerp(pow(input.uv.y - _shadowOcclusionHeight, _shadowOcclusion), 1.0, shadowAtten));
                float windMask = smoothstep(0.01, 0.5, smoothstep(0.01, 0.3, input.windStrength) * 0.2);

                // half3 color = groundColor * _Intensity + windMask;
                half3 color = groundColor;
                // color = lerp(_ShadowColor, color, occlusion);
                // color = lerp(color, _LightColor, NdotL * _BaseOffsetY);
                // color = lerp(color, _WindColor, smoothstep(0.04, 0.1, windMask));
                // color += step(0.09, input.windStrength) * _Intensity;
                // color = MixFog(color, input.fogCoord);
                
                // float2 spriteUV = GetGrassSpriteUV(input.uv, input.positionWS, .1);
                float4 grass = tex2D(_GrassMask, input.uv - float2(0,.1));
                float alpha = smoothstep(_Edge0, _Edge1, grass.a  - _Alpha);
                clip(alpha - _Alpha - .9);

                float flowerMask = step( .1, grass);
                float3 grassColor = lerp(color, grass, flowerMask);
                return float4(grassColor.rgb,1 );
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Nature/SpeedTree7"
}