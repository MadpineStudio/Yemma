Shader "Custom/Leaves"
{
    Properties
    {
        _Angle("Angle", Float) = 0
        _Pivot("_Pivot", Vector) = (0,0,0,0)
        _Alpha("Alpha", Range(0,1)) = .1
        _BaseTex("BaseTex", 2D) = "white" {}
        _Color("Color", Color) = (0,0,0,0)
        _LightColor("LightColor", Color) = (0,0,0,0)
        _BrightLightColor("BrightLightColor", Color) = (0,0,0,0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        
        Cull Off
        LOD 100
        ZWrite On
        ZTest LEqual
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
                float3 normalWS     : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
            float _Angle;
            float _Alpha;
            float4 _Pivot;
            sampler2D _BaseTex;
            float4 _Color;
            float4 _LightColor;
            float4 _BrightLightColor;
            
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            void ApplyVertexBillboard(inout float3 positionOS, float3 quadCenter)
            {
                float3 worldQuadCenter = mul(UNITY_MATRIX_M, float4(quadCenter, 1.0)).xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldQuadCenter);
                float3 up = float3(0, 1, 0);
                float verticalMask = 0.0;
                
                viewDir.y *= (1.0 - verticalMask);
                viewDir = normalize(viewDir + float3(0, 0.0001, 0));
                
                float3 right = normalize(cross(up, viewDir));
                float3 forward = normalize(cross(right, up));
                
                float3 localOffset = positionOS - quadCenter;
                positionOS = quadCenter 
                    + localOffset.x * right 
                    + localOffset.y * up 
                    + localOffset.z * forward;
            }

            float4 ConvertVertexColor(float4 originalColor)
            {
                float3 linearColor = Gamma22ToLinear(originalColor.rgb);
                return float4(linearColor * 2 - 1, originalColor.a);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Converter cor do vértice para obter pivô
                float4 p = -ConvertVertexColor(input.color);
                float3 quadCenterOS = p.xzy;
                
                // Aplicar billboard apenas à posição do vértice
                // ApplyVertexBillboard(input.positionOS.xyz, quadCenterOS);
                
                // Transformações normais
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                
                // Manter a normal original no world space (ignorando o billboard)
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                output.uv = input.uv;
                output.color = p;
                
                return output;
            }

            // Iluminação fixa (não depende da rotação do billboard)
            float FixedLighting(float3 worldNormal)
            {
                Light mainLight = GetMainLight();
                // Você pode substituir mainLight.direction por uma direção fixa se preferir
                // Exemplo: float3 fixedLightDir = normalize(float3(0.5, 1, 0.5));
                float NdotL = saturate(dot(worldNormal, mainLight.direction) + 1);
                return NdotL; 
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float4 col = tex2D(_BaseTex,  input.uv);
                clip(col.a - _Alpha);
                    
                // float light = FixedLighting(normalize(input.normalWS)) - .2;
                // light = floor(light * _Pivot.x) / (_Pivot.x - 1);
                //
                //
                // float base = 1. - smoothstep(_Pivot.z, _Pivot.y, light);
                // float3 color = lerp(_Color, _LightColor, base);
                //
                // float t = step( _Pivot.w, light);
                //
                // color = lerp(color, _BrightLightColor, t);
                return half4(col.xyz, 1);
            }
            ENDHLSL
        }
        
        // Passo adicional para sombras
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}