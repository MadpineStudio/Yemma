// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ComputeGeneratedMesh"
{
    Properties
    {
       
        _GrassScale("_GrassScale", Float) = 1
        _WindScale("_WindScale", Range(.0001, 3)) = .1
        _WindStrength("_WindStrength", Float) = .1
        _WindVelocity("_WindVelocity", Vector) = (0,0,0,0)

        _BaseOffsetY("_BaseOffsetY", Float) = .1

        _DarkenColor("_DarkenColor", Color) = (1,1,1,1)
        _Color("Color", Color) = (1,1,1,1)
        
        _WindTex("WindTex", 2D) = "white" {} 
        _GrassMask("GrassMask", 2D) = "white" {}
        _GrassGroundMask("grass ground Mask", 2D) = "white" {} 
        

    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        Cull Off
        ZWrite On
        ZClip On
        ZTest LEqual
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert 
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            cbuffer GrassData
            {
                sampler2D _WindTex;
                sampler2D _GrassMask;
                sampler2D _GrassGroundMask;
                
                float2 _WindVelocity;
                float _WindStrength;
                float _WindScale;
                float _GrassScale;
                
                float _BaseOffsetY;
                
                float3 _DarkenColor;
                float3 _Color;
                float _Angle; // Ângulo de rotação
            }
            
            struct DrawVertex {
                float3 positionWS;
                float2 uv;
            };

            struct DrawTriangle {
                float3 normalWS;
                DrawVertex vertices[3];
            };

            StructuredBuffer<DrawTriangle> _DrawTriangles;
            
            struct VSInput {
                uint vertexID : SV_VertexID;
            };

            struct PSInput {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normals : TEXCOORD1;
                float wind: TEXCOORD2;
                float3 posWS : TEXCOORD3;
            };

            // Função para calcular a matriz de rotação
            float3x3 CalculaNovaRotacaoMatrix(float3 axis, float angle)
            {
                float3 u = normalize(axis);
                float s = sin(angle);
                float c = cos(angle);
                float t = 1 - c;
                return float3x3(
                    t * u.x * u.x + c,     t * u.x * u.y - s * u.z,   t * u.x * u.z + s * u.y,
                    t * u.x * u.y + s * u.z, t * u.y * u.y + c,         t * u.y * u.z - s * u.x,
                    t * u.x * u.z - s * u.y, t * u.y * u.z + s * u.x,   t * u.z * u.z + c
                );
            }

            PSInput vert(VSInput input)
            {
                PSInput o;
                int triIndex = input.vertexID / 3;
                int vertIndex = input.vertexID % 3;
                
                DrawTriangle tri = _DrawTriangles[triIndex];
                DrawVertex v = tri.vertices[vertIndex];
                o.uv = v.uv;
                o.posWS = v.positionWS;
                // int quadIndex = triIndex / 2; // Cada quad tem 2 triângulos
                // int triInQuad = triIndex % 2; // Índice do triângulo no quad (0 ou 1)
                
                o.positionCS = UnityObjectToClipPos(float4(v.positionWS, 1.));

                // Calcula o vento
                float wind = v.uv.y * tex2Dlod(_WindTex, float4(o.posWS.xz * _WindScale + _WindVelocity * _Time.y ,1, 1));
                // wind = pow(wind, 2);
                wind = pow(wind, _WindStrength);
                o.positionCS.xz += wind;
                
                o.normals = tri.normalWS;
                o.wind = wind;
                return o;
            }

            float4 frag(PSInput i) : SV_Target
            {
                //wind
                float windMask = tex2D(_WindTex,float2(i.posWS.xz * _WindScale + _WindVelocity * _Time.y));
                // windMask = pow(windMask, 2);
                windMask = pow(windMask, _WindStrength);

                float blade = tex2D(_GrassMask, i.uv - float2(0., .1));
                float3 ground = tex2D(_GrassGroundMask, i.posWS.xz * _GrassScale);
                clip(blade - .1);
                float baseGradient = i.uv.y + _BaseOffsetY;
                half3 color = lerp(_DarkenColor, ground, baseGradient);
                // return float4(windMask.xxx, 1);
                return float4(color, 1);
            }
            ENDCG
        }
    }
}