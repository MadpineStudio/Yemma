Shader "Unlit/S_GrassRenderDisplace"
{
    Properties
    {
        _RenderTex ("_RenderTex", 2D) = "white" {}
        _Scale("_Scale", Float) = .1
        _Scale01("_Scale01", Float) = .1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _RenderTex;
            float _Scale;
            float _Scale01;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float circle = smoothstep(_Scale, _Scale01, length(i.uv - .5));
                return float4(1,0,0, circle);
            }
            ENDCG
        }
    }
}
