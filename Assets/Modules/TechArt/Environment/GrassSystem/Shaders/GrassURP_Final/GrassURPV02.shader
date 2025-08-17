Shader "Custom/OptimizedGrassRendererURP"
{
    Properties
    {
        _GrassMask("Grass Mask", 2D) = "white" {}
        _GrassGroundMask("Grass Ground Mask", 2D) = "white" {}
        _Angle("Angle", Range(0,.1)) = 0
        X("X", Range(0,10)) = 0
        Z("Z", Range(0,10)) = 0
        _Intensity("Intensity", Range(0,10)) = 0
        _Alpha("Alpha", Range(0,1)) = 0
        _Pivot("Pivot", Vector) = (0,0,0,0) 
    }

    SubShader
    {
        Tags { 
            "RenderType" = "TransparentCutout"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        
        Cull Off
        LOD 0
        ZWrite On
        ZTest On

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
                float3 positionWS   : TEXCOORD1;
            };

            sampler2D _GrassMask;
            sampler2D _GrassGroundMask;

            CBUFFER_START(UnityPerMaterial)
            float _Angle;
            float2 _Pivot;
            float X;
            float Z;
            float _Intensity;
            float _Alpha;
            CBUFFER_END

            StructuredBuffer<DrawTriangle> _DrawTriangles;

            float2 RotateUV(float2 uv, float angle, float2 pivot)
            {
                float sinAngle = sin(angle);
                float cosAngle = cos(angle);
                
                uv -= pivot;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosAngle - uv.y * sinAngle;
                rotatedUV.y = uv.x * sinAngle + uv.y * cosAngle;
                rotatedUV += pivot;
                
                return rotatedUV;
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

                output.uv = v.uv;
                output.positionWS = v.positionWS;
                output.positionCS = TransformWorldToHClip(output.positionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 x = input.positionWS.xz * _Angle + float2(X,Z);
                float2 uvs = RotateUV(x, -80 * PI / 180, _Pivot);

                float3 groundColor = tex2D(_GrassGroundMask, uvs).rgb;
                float4 grass = tex2D(_GrassMask, input.uv - float2(0,.1));
                clip(grass.a - _Alpha);

                float flowerMask = step(.1, grass);
                float3 grassColor = lerp(groundColor.rgb, grass.rgb, flowerMask);

                return float4(grassColor * _Intensity, 1);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Nature/SpeedTree7"
}