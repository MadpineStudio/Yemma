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
        
        // Novos parâmetros do shader atualizado
        _CoreSize("Core Size", Range(0.001, 0.05)) = 0.010
        _HaloSize("Halo Size", Range(0.01, 0.15)) = 0.070
        _FringeSize("Fringe Size", Range(0.01, 0.08)) = 0.030
        _Dispersion("RGB Dispersion", Range(0.0, 0.1)) = 0.030
        _EndWidth("End Flare Width", Range(0.02, 0.2)) = 0.08
        _Jitter("Chromatic Jitter", Range(0.0, 0.02)) = 0.008
        _SparkThreshold("Spark Threshold", Range(0.5, 0.95)) = 0.82
        _SparkIntensity("Spark Intensity", Range(0.0, 3.0)) = 1.2
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
                float _Maximum;
                float _FadeDistance;
                float _CoreSize;
                float _HaloSize;
                float _FringeSize;
                float _Dispersion;
                float _EndWidth;
                float _Jitter;
                float _SparkThreshold;
                float _SparkIntensity;
            CBUFFER_END
            
            // PropertyBlock compatible properties
            float4 _StartPoint;
            float4 _EndPoint;

            // Hash rápido (baseado no original)
            float h21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Gauss 1D
            float gauss(float x, float s)
            {
                return exp(-0.5 * (x * x) / (s * s + 1e-6));
            }

            // Conversão hue to RGB (mantido do shader original)
            float3 hue2rgb(float h)
            {
                float3 k = float3(1.0, 2.0/3.0, 1.0/3.0);
                return saturate(abs(frac(h + k) * 6.0 - 3.0) - 1.0);
            }

            float3 prismLaser(float2 st, float3 worldPos)
            {
                // Calculate distance fade based on world position
                float distToStart = distance(worldPos, _StartPoint.xyz);
                float distToEnd = distance(worldPos, _EndPoint.xyz);
                float fadeStart = smoothstep(0.0, _FadeDistance, distToStart);
                float fadeEnd = smoothstep(0.0, _FadeDistance, distToEnd);
                float totalFade = fadeStart * fadeEnd;
                
                float y = st.y - 0.5;
                float x = st.x;

                // Envelopes (usando parâmetros configuráveis)
                float coreS = _CoreSize;
                float haloS = _HaloSize;
                float fringeS = _FringeSize;
                float disp = _Dispersion;
                float endW = _EndWidth;
                float jitter = _Jitter;

                // Taper nas extremidades para "flares" coloridos
                float ends = 1.0 - smoothstep(0.0, endW, x) - smoothstep(0.0, endW, 1.0 - x);
                ends = clamp(ends, 0.0, 1.0);

                // Jitter por-pixel nas amostras (grão nas bordas)
                float j = (h21(st * 100.0 + 17.0) - 0.5) * jitter;

                // Núcleo branco
                float core = gauss(y, coreS);
                // Halo suave
                float glow = gauss(y, haloS);

                // Franjas cromáticas (R sobe, B desce, G centro) com jitter
                float rB = gauss(y - (disp + j), fringeS);
                float gB = gauss(y - (0.00 + j), fringeS * 1.1);
                float bB = gauss(y - (-disp + j), fringeS);

                // Anel: região "entre" halo e núcleo (onde mora o ruído e cor)
                float ring = clamp(glow - core, 0.0, 1.0);

                // Granulação colorida nas extremidades do feixe
                float grain = h21(floor(st * float2(60.0, 100.0)) + 73.0);
                float spark = step(_SparkThreshold, grain) * ring * (ends * 1.5 + 0.25);
                float3 sparkCol = float3(h21(st + 2.3), h21(st + 5.7), h21(st + 9.1));
                sparkCol = lerp(float3(1.0, 0.2, 0.9), sparkCol, 0.35); // viés magenta

                // Composição
                float3 col = float3(0.0, 0.0, 0.0);
                col += float3(1.0, 1.0, 1.0) * core * 1.8;                    // miolo quente
                col += float3(rB, gB, bB) * ring * 1.2;                        // dispersão
                col += float3(0.9, 0.4, 1.2) * pow(glow, 1.35) * 0.55;        // bloom suave
                col += sparkCol * spark * _SparkIntensity;                     // grão cromático nas bordas

                // Leve vinheta vertical para destacar a faixa
                float vign = smoothstep(0.0, 0.85, 1.0 - abs(y) * 1.6);
                col *= vign;

                // Aplicar fade baseado na distância dos pontos
                col *= totalFade;

                // Aplicar intensidade geral
                col *= _Intensity;

                return col;
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
                float3 laser = prismLaser(IN.uv, IN.worldPos);
                float3 res = bg + laser;

                // Alpha baseado na intensidade total do laser
                float alpha = saturate(laser.r + laser.g + laser.b);
                
                return half4(res, alpha);
            }

            ENDHLSL
        }
    }
}