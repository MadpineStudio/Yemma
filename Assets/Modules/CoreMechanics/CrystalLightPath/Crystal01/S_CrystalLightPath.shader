Shader "Custom/S_CrystalLightPath"
{
    Properties
    {
        _Width("Width", Range(0.01, 0.2)) = 0.06
        _Intensity("Intensity", Range(0.1, 3.0)) = 1.3
        _BackgroundColor("Background Color", Color) = (0.08, 0.08, 0.08, 1)
        _Minimum("Minimum Hue", Range(-2.0, 2.0)) = 0.75
        _Maximum("Maximum Hue", Range(-2.0, 2.0)) = 0.95
        _StartPoint("Start Point", Vector) = (0, 0, 0, 0)
        _EndPoint("End Point", Vector) = (1, 1, 1, 0)
        _FadeDistance("Fade Distance", Range(0.1, 2.0)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Width;
                float _Intensity;
                half4 _BackgroundColor;
                float _Minimum;
                float _Maxmum;
                float _FadeDistance;
            CBUFFER_END
            
            // PropertyBlock compatible properties
            float4 _StartPoint;
            float4 _EndPoint;

            float3 hue2rgb(float h)
            {
                float3 k = float3(1.0, 2.0/3.0, 1.0/3.0);
                return saturate(abs(frac(h + k) * 6.0 - 3.0) - 1.0);
            }

            float3 prism(float2 st, float w, float3 worldPos)
            {
                // Calculate distance fade based on world position
                float distToStart = distance(worldPos, _StartPoint.xyz);
                float distToEnd = distance(worldPos, _EndPoint.xyz);
                float fadeStart = smoothstep(0.0, _FadeDistance, distToStart);
                float fadeEnd = smoothstep(0.0, _FadeDistance, distToEnd);
                float totalFade = fadeStart * fadeEnd;
                
                float d = st.y - 0.5;
                float ad = abs(d);

                float core = smoothstep(w, 0.0, ad);
                float halo = smoothstep(w * 3.0, 0.0, ad) - core;

                float t = saturate(0.5 + d / (w * 1.4));
                float h = lerp(_Minimum, _Maxmum, t);
                float3 col = hue2rgb(h);

                float whiteMix = core * core;
                float3 band = lerp(col, float3(1.0, 1.0, 1.0), whiteMix) * core + col * halo * 0.30;
                return band * totalFade;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 bg = _BackgroundColor.rgb;
                float3 res = bg + prism(IN.uv, _Width, IN.worldPos) * _Intensity;

                return half4(res, res.x + res.y + res.z);
            }

            ENDHLSL
        }
    }
}