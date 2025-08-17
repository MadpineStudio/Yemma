Shader "Custom/AnimeEyeParallax"
{
    Properties
    {
        _ParallaxStrength ("Parallax Strength", Float) = 0.1
        _Radius ("Radius", Range(0,1)) = 0.1
        _RadiusInner("_RadiusInner", Range(0,1)) = .1
        _Color("_Color", Color) = (0,0,0,0)
        _ColorIris("_ColorIris", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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
                float3 normal : NORMAL;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
            };

            float _ParallaxStrength;
            float _Radius;
            float _RadiusInner;
            half3 _Color;
            half3 _ColorIris;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                o.viewDir = mul(unity_WorldToObject, float4(o.viewDir, 0)).xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 irisUV = i.uv + (i.viewDir.xy * _ParallaxStrength);
                float r = step(length(irisUV -.5),  _Radius);
                float outline = step(length(irisUV - .5), _RadiusInner);
                float3 result = lerp(r * _ColorIris, _Color, outline);

                float iris = step(length(irisUV - .5), .05);
                float t = step(length(irisUV - .5 - float2(.0,-.07)), .15);
                result = lerp(result, 0, iris);
                result *= 1.-irisUV.y * 1.3;
                    
                return float4(result, 1);
            }
            ENDCG
        }
    }
}