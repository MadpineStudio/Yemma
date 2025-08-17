Shader "Universal Render Pipeline/SimpleTerrain"
{
    Properties
    {
        // Texturas do Terrain (control + splats)
        [HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "grey" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "grey" {}

        // Heightmap (necessário para o terreno não ficar plano)
        [HideInInspector] _TerrainHeightmapTexture("Heightmap", 2D) = "black" {}
        [HideInInspector] _TerrainHeightmapScale("Heightmap Scale", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue" = "Geometry-100" "RenderType" = "Opaque" }

        Pass
        {
            Name "SimpleTerrain"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Texturas
            TEXTURE2D(_Control);
            SAMPLER(sampler_Control);
            TEXTURE2D(_Splat0);
            SAMPLER(sampler_Splat0);
            TEXTURE2D(_Splat1);
            SAMPLER(sampler_Splat1);
            TEXTURE2D(_Splat2);
            SAMPLER(sampler_Splat2);
            TEXTURE2D(_Splat3);
            SAMPLER(sampler_Splat3);

            // Heightmap
            TEXTURE2D(_TerrainHeightmapTexture);
            SAMPLER(sampler_TerrainHeightmapTexture);
            float _TerrainHeightmapScale;

            // Aplica o displacement do heightmap
            float3 ApplyTerrainHeight(float3 positionOS, float2 uv)
            {
                float height = SAMPLE_TEXTURE2D_LOD(_TerrainHeightmapTexture, sampler_TerrainHeightmapTexture, uv, 0).r;
                positionOS.y += height * _TerrainHeightmapScale;
                return positionOS;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                
                // Aplica o heightmap para o terreno não ficar plano
                float3 displacedPos = ApplyTerrainHeight(v.positionOS.xyz, v.texcoord);
                o.positionCS = TransformObjectToHClip(displacedPos);
                o.uv = v.texcoord;
                
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Mistura as splatmaps baseado no controle
                half4 control = SAMPLE_TEXTURE2D(_Control, sampler_Control, i.uv);
                half4 splat0 = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, i.uv);
                half4 splat1 = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat1, i.uv);
                half4 splat2 = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat2, i.uv);
                half4 splat3 = SAMPLE_TEXTURE2D(_Splat3, sampler_Splat3, i.uv);

                half4 color = splat0 * control.r;
                color += splat1 * control.g;
                color += splat2 * control.b;
                color += splat3 * control.a;

                return color;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}