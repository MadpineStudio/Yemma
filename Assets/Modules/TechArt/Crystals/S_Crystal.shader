Shader "Custom/S_Crystal"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        
        [Header(Gradient Settings)]
        _GradientColorTop("Gradient Color Top", Color) = (1, 0.5, 0.8, 1)
        _GradientColorBottom("Gradient Color Bottom", Color) = (0.2, 0.1, 0.8, 1)
        _GradientCenter("Gradient Center", Range(0, 1)) = 0.5
        _GradientPower("Gradient Power", Range(0.1, 5)) = 1
        _GradientRotation("Gradient Rotation", Range(0, 360)) = 0
        
        [Header(Noise Settings)]
        _NoiseScale("Noise Scale", Range(0.1, 10)) = 1
        _NoiseIntensity("Noise Intensity", Range(0, 1)) = 0.2
        
        [Header(Fresnel Settings)]
        _FresnelPower("Fresnel Power", Range(0.1, 10)) = 2
        _FresnelIntensity("Fresnel Intensity", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _GradientColorTop;
                half4 _GradientColorBottom;
                float _GradientCenter;
                float _GradientPower;
                float _GradientRotation;
                float _NoiseScale;
                float _NoiseIntensity;
                float _FresnelPower;
                float _FresnelIntensity;
            CBUFFER_END

            // Função para rotacionar coordenadas UV
            float2 RotateUV(float2 uv, float rotation)
            {
                float rad = radians(rotation);
                float cosRot = cos(rad);
                float sinRot = sin(rad);
                
                // Centraliza UV em (0,0) para rotação
                uv -= 0.5;
                
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosRot - uv.y * sinRot;
                rotatedUV.y = uv.x * sinRot + uv.y * cosRot;
                
                // Retorna ao centro (0.5, 0.5)
                return rotatedUV + 0.5;
            }

            // Função de ruído simples para variação
            float SimpleNoise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Função de ruído suavizado
            float SmoothNoise(float2 uv, float scale)
            {
                uv *= scale;
                float2 i = floor(uv);
                float2 f = frac(uv);
                
                // Smoothstep para interpolação suave
                f = f * f * (3.0 - 2.0 * f);
                
                float a = SimpleNoise(i);
                float b = SimpleNoise(i + float2(1.0, 0.0));
                float c = SimpleNoise(i + float2(0.0, 1.0));
                float d = SimpleNoise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Função de gradiente linear básico
            float LinearGradient(float2 uv, float center, float power)
            {
                float gradient = (uv.y - center) + 0.5;
                return pow(saturate(gradient), power);
            }

            // Função de gradiente radial
            float RadialGradient(float2 uv, float2 center, float power)
            {
                float dist = distance(uv, center);
                return pow(saturate(1.0 - dist), power);
            }

            // Função de gradiente diagonal
            float DiagonalGradient(float2 uv, float power)
            {
                float diagonal = (uv.x + uv.y) * 0.5;
                return pow(saturate(diagonal), power);
            }

            // Função principal de gradiente que combina todas as outras
            half3 CalculateCrystalGradient(float2 uv, float3 worldPos, float3 normal, float3 viewDir)
            {
                // Rotaciona UV se necessário
                float2 rotatedUV = RotateUV(uv, _GradientRotation);
                
                // Calcula gradiente linear básico
                float gradientMask = LinearGradient(rotatedUV, _GradientCenter, _GradientPower);
                
                // Adiciona ruído para variação
                float noise = SmoothNoise(worldPos.xz, _NoiseScale);
                gradientMask = lerp(gradientMask, gradientMask * noise, _NoiseIntensity);
                
                // Interpola entre as cores do gradiente
                half3 gradientColor = lerp(_GradientColorBottom.rgb, _GradientColorTop.rgb, gradientMask);
                
                return gradientColor;
            }

            // Função para calcular efeito Fresnel
            float CalculateFresnel(float3 normal, float3 viewDir, float power)
            {
                float fresnel = 1.0 - saturate(dot(normalize(normal), normalize(viewDir)));
                return pow(fresnel, power);
            }

            // Função para combinar gradiente com fresnel
            half3 CombineGradientWithFresnel(half3 gradientColor, float3 normal, float3 viewDir)
            {
                float fresnel = CalculateFresnel(normal, viewDir, _FresnelPower);
                half3 fresnelColor = lerp(gradientColor, _GradientColorTop.rgb, fresnel * _FresnelIntensity);
                return fresnelColor;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                
                // Calcula posição no espaço mundial
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                
                // Calcula normal no espaço mundial
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                // Calcula direção da câmera
                OUT.viewDirWS = GetWorldSpaceViewDir(OUT.positionWS);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Amostra a textura base
                half4 baseTexture = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                
                // Calcula o gradiente do cristal
                half3 crystalGradient = CalculateCrystalGradient(IN.uv, IN.positionWS, IN.normalWS, IN.viewDirWS);
                
                // Combina gradiente com efeito Fresnel
                half3 finalGradient = CombineGradientWithFresnel(crystalGradient, IN.normalWS, IN.viewDirWS);
                
                // Combina tudo com a cor base e textura
                half3 finalColor = baseTexture.rgb * _BaseColor.rgb * finalGradient;
                
                return half4(finalColor, _BaseColor.a * baseTexture.a);
            }
            ENDHLSL
        }
    }
}
