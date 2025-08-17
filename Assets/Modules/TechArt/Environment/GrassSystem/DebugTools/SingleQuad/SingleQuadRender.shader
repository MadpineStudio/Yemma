
Shader "Custom/ComputeGeneratedMesh"
{
    Properties
    {
        _Tex("Tex", 2D) = "white" {}
        _BaseColor("_BaseColor", Color) = (0,0,0,0)
        _Color("_Color", Color) = (0,0,0,0)
        _angle("_angle", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
//          Blend SrcAlpha OneMinusSrcAlpha
          LOD 100
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
            
            cbuffer GrassData4
            {
                sampler2D _Tex;
                float3 _BaseColor;
                float3 _Color;
                float _angle;
            }
            
            struct DrawVertex {
                float3 positionWS;
                float2 uv;  
            };

            struct DrawTriangle {
                float3 normalWS;
                float3 normalAlignToSurface;    // 3 floats
                DrawVertex vertices[4];
            };

            StructuredBuffer<DrawTriangle> _DrawTriangles;
            
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
            float3x3 BillboardAxis(float3 position, float3 axis)
            {
                axis = normalize(axis);
                axis.y += _angle * 3.1415/180;
                // axis.y *= -1;
                float3 toCam = position - _WorldSpaceCameraPos.xyz  ;
                float3 proj = toCam - dot(toCam, axis) * axis;
                proj = normalize(proj);
                float3 forward = -proj;
                float3 right = normalize(cross(axis, forward));
                float3 up = cross(forward, right);
                return float3x3(-right, up, forward);
            }
            struct VSInput {
                uint vertexID : SV_VertexID;
            };

            struct PSInput {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normals : TEXCOORD1;
                float3 posWS : TEXCOORD2;
            };

            PSInput vert(const VSInput input)
            {
                PSInput o;
                int triIndex = input.vertexID / 4;
                int vertIndex = input.vertexID % 4;

                const DrawTriangle tri = _DrawTriangles[triIndex];
                DrawVertex v = tri.vertices[vertIndex];
                
                o.uv = v.uv;
                o.posWS = v.positionWS ;

                float3 centerWS = (tri.vertices[0].positionWS + tri.vertices[1].positionWS + tri.vertices[2].positionWS + tri.vertices[3].positionWS ) / 4.0;
                float3 localVertex = v.positionWS;

                float3x3 angleRotation = CalculaNovaRotacaoMatrix(tri.normalAlignToSurface, _angle * 3.1415 / 180);
                float3x3 billboardOnly_Y = BillboardAxis(localVertex, float3(0,1,0));
                localVertex = mul(billboardOnly_Y, localVertex - centerWS) + centerWS;
                
                o.positionCS = UnityObjectToClipPos(float4(localVertex , 1.));
                o.normals = tri.normalWS;
                return o;
            }

            float4 frag(const PSInput i) : SV_Target
            {
                float c = step(length(i.uv - .5), .03);
                float grassMask = tex2D(_Tex, i.uv - float2(0,.01));
                clip(grassMask - .2);
                // return float4(i.uv, 0, 1);
                return float4(lerp(_BaseColor, _Color, i.uv.y), 1);
            }
            ENDCG
        }
    }
}