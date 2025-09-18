Shader "Custom/S_CrystalParallax"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _ParallaxStrength("Parallax Strength", Range(0, 0.1)) = 0.02
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
                float3 viewDirWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _ParallaxStrength;
            CBUFFER_END

            // Função de parallax com correção para ângulos rasantes (grazing angles)
            float2 CalculateSimpleParallax(float2 uv, float3 viewDirWS, float3 normalWS)
            {
                // Normaliza as direções
                float3 viewDir = normalize(viewDirWS);
                float3 normal = normalize(normalWS);
                
                // Calcula o ângulo entre a view direction e a normal
                float NdotV = saturate(dot(normal, viewDir));
                
                // Fator de atenuação para ângulos rasantes
                // Quando NdotV é próximo de 0 (ângulo raso), reduz o efeito drasticamente
                float angleFactor = smoothstep(0.0, 0.3, NdotV);
                
                // Calcula offset com correção baseada no ângulo
                float2 parallaxOffset = viewDir.xy * _ParallaxStrength * angleFactor;
                return uv + parallaxOffset;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                
                // Calcula direção da view no world space
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionWS);
                
                // Calcula normal no world space para correção de ângulos
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Calcula UV com parallax corrigido para ângulos rasantes
                float2 parallaxUV = CalculateSimpleParallax(IN.uv, IN.viewDirWS, IN.normalWS);
                
                // Amostra a textura com UV modificado pelo parallax
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, parallaxUV) * _BaseColor;
                half4 overlay = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * float4(0,0,1,0);
                
                return overlay + color;
            }
            ENDHLSL
        }
    }
}
