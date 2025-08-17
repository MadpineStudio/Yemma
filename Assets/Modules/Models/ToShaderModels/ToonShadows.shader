Shader "Unlit/ToonShadows"
{
    Properties
    {
        _ShadowTex ("_ShadowTex", 2D) = "white" {}
        _MainLightZEuler("_MainLightZEuler", Float) = 0
        _HeadZEuler("_HeadZEuler", Float) = 0
        _Threshold("_Threshold", Range(0,1)) = .5
        
        _ShadowColor("_ShadowColor", Color) = (0,0,0,0)
        _LightColor("_ShadowColor", Color) = (0,0,0,0)
        _LightSmoothTransition("_LightSmoothTransition", Range(0,1)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite On
        ZTest LEqual
        ZClip Off
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
            
            sampler2D _ShadowTex;
            float _MainLightZEuler;
            float _HeadZEuler;
            float _Threshold;
            float _LightSmoothTransition;
            
            float3 _ShadowColor;
            float3 _LightColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float CalculateShadowToonMask(float2 uv)
            {
                float d = (_HeadZEuler - _MainLightZEuler) / (3.14159 * 2.0);
                float f = frac(d);
                float isLess = 1.0 - step(_Threshold, f);
                float isGreater = step(_Threshold, f);
                float sum = isLess - isGreater;
                float f2 = abs(isGreater - f);
                float2 newUV = uv * float2(sum, 1.0);
                float col = clamp(tex2D(_ShadowTex, newUV).r, 0.0, 1.0);
                col = 1.0 - step(f2, col);
                return col;
            }
            float3 CalculateColors(float gradient)
            {
                float3 color = lerp(_LightColor, _ShadowColor , gradient);
                return color;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float shadows = CalculateShadowToonMask(i.uv);
                float3 colors = CalculateColors(shadows);
                return float4(colors, 1);
            }
            ENDCG
        }
    }
}
