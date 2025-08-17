// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ComputeGeneratedMesh"
{
    Properties
    {
        _DarkenColor("_DarkenColor", Color) = (1,1,1,1)
        _Color("Color", Color) = (1,1,1,1)
        _Radius("Radius", Range(0,1)) = .1
        _Smooth("Radius", Range(0,1)) = .1
        _WindVelocity("_WindVelocity", Vector) = (0,0,0,0)
        _WindStrength("_WindStrength", Float) = .1
        _WindScale("_WindScale", Range(.0001, 3)) = .1
        _WindTex("WindTex", 2D) = "white" {} 
        _GrassMask("GrassMask", 2D) = "white" {} 
        _Angle("Angle", Float) = 0.0 // Ângulo de rotação
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
                float2 _WindVelocity;
                float _WindStrength;
                float _WindScale;
                
                float _Radius;
                float _Smooth;
                
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

                // Calcula o centro do quad (média dos vértices do quad)
                // Cada quad é composto por dois triângulos, então precisamos agrupar os vértices corretamente.
                // int quadIndex = triIndex / 2; // Cada quad tem 2 triângulos
                // int triInQuad = triIndex % 2; // Índice do triângulo no quad (0 ou 1)
                //
                // // Obtém os vértices do quad
                DrawTriangle tri0 = _DrawTriangles[0];
                // DrawTriangle tri1 = _DrawTriangles[4];
                // DrawTriangle tri1 = _DrawTriangles[quadIndex + 1];

                // Calcula o centro do quad (média dos 4 vértices)
                // float3 centerWS = (tri0.vertices[0].positionWS + tri0.vertices[1].positionWS + tri0.vertices[2].positionWS ) / 3.0;
                // // Calcula a matriz de rotação em torno do eixo Y (ou outro eixo, se necessário)
                //
                // // Aplica a rotação em torno do centro do quad

                // Transforma a posição para o espaço de projeção
                // float3 centerWS = (tri0.vertices[0].positionWS + tri0.vertices[1].positionWS + tri0.vertices[2].positionWS ) / 3.0;
                // float3 centerWS = (tri0.vertices[0].positionWS + tri0.vertices[1].positionWS + tri0.vertices[2].positionWS + tri1.vertices[1].positionWS) / 4 ;
                // float3x3 vertexRot = CalculaNovaRotacaoMatrix(float3(0, 1, 0), _Angle * 3.1415 / 180.0);
                // float3 localVertex = mul(unity_WorldToObject, float4(v.positionWS, 1));
                // localVertex = mul(vertexRot, localVertex - centerWS) + centerWS;
                // localVertex = mul(unity_ObjectToWorld, float4(localVertex, 1.));
                //
                o.positionCS = UnityObjectToClipPos(float4(v.positionWS, 1.));

                // Calcula o vento
                float wind = tex2Dlod(_WindTex, float4(o.posWS.xz * _WindScale + _WindVelocity * _Time.y ,1, 1));
                wind = pow(wind, 2);
                wind = pow(wind, _WindStrength);
                
                o.normals = tri.normalWS;
                o.wind = wind;
                return o;
            }

            float4 frag(PSInput i) : SV_Target
            {
                float g = step(length(i.uv - float2(.5, .15)), i.wind * .2 + _Radius);
                float gColor = saturate(pow(i.wind.xxx * 4, 4));
                float3 col = lerp(_DarkenColor, _Color, gColor);

                float blade = tex2D(_GrassMask, i.uv - float2(0., .1));
                clip(blade - .1);
                return float4(col, g);
            }
            ENDCG
        }
    }
}