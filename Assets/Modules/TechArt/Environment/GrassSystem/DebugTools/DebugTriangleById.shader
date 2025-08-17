Shader "Custom/PlaneTiltNoise"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Estruturas
            struct appdata
            {
                float4 vertex : POSITION; // Vértice no espaço local
                float2 uv : TEXCOORD0;   // Coordenadas UV
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // Posição no espaço de clip
                float2 uv : TEXCOORD0;   // Coordenadas UV
            };
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(float4(v.vertex.xyz, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 1;
            }
            ENDCG
        }
    }
}