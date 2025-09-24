Shader "Custom/S_MatCapGold"
{
    Properties
    {
        _MatCapTexture("MatCap Texture", 2D) = "white" {}
        _MatCapIntensity("MatCap Intensity", Range(0, 3)) = 1
        _MatCapContrast("MatCap Contrast", Range(0.1, 2)) = 1
        _MatCapSaturation("MatCap Saturation", Range(0, 2)) = 1
        _MatCapRotation("MatCap Rotation", Range(0, 360)) = 0
        
        [Toggle] _UseMask("Use Mask", Float) = 0
        _MaskTexture("Mask Texture", 2D) = "white" {}
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
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            TEXTURE2D(_MatCapTexture);
            SAMPLER(sampler_MatCapTexture);
            
            TEXTURE2D(_MaskTexture);
            SAMPLER(sampler_MaskTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MatCapTexture_ST;
                float4 _MaskTexture_ST;
                float _MatCapIntensity;
                float _MatCapContrast;
                float _MatCapSaturation;
                float _MatCapRotation;
                float _UseMask;
            CBUFFER_END

            float2 RotateUV(float2 uv, float rotation)
            {
                float rad = radians(rotation);
                float cosRot = cos(rad);
                float sinRot = sin(rad);
                uv -= 0.5;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosRot - uv.y * sinRot;
                rotatedUV.y = uv.x * sinRot + uv.y * cosRot;
                return rotatedUV + 0.5;
            }

            float3 AdjustMatCapColor(float3 color, float intensity, float contrast, float saturation)
            {
                color *= intensity;
                color = pow(color, contrast);
                float gray = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(gray, color, saturation);
            }

            float2 CalculateMatCapUV(float3 normalWS, float3 viewDirWS)
            {
                float3 normalVS = TransformWorldToViewDir(normalWS, true);
                float3 viewDirVS = TransformWorldToViewDir(viewDirWS, true);
                float3 reflectionVS = reflect(-viewDirVS, normalVS);
                float2 matCapUV = reflectionVS.xy * 0.5 + 0.5;
                return RotateUV(matCapUV, _MatCapRotation);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 matCapUV = CalculateMatCapUV(IN.normalWS, IN.viewDirWS);
                half3 matCapColor = SAMPLE_TEXTURE2D(_MatCapTexture, sampler_MatCapTexture, matCapUV).rgb;
                matCapColor = AdjustMatCapColor(matCapColor, _MatCapIntensity, _MatCapContrast, _MatCapSaturation);
                
                if (_UseMask > 0.5)
                {
                    float mask = SAMPLE_TEXTURE2D(_MaskTexture, sampler_MaskTexture, TRANSFORM_TEX(IN.uv, _MaskTexture)).r;
                    matCapColor *= mask;
                }
                
                return half4(matCapColor, 1);
            }
            ENDHLSL
        }
    }
}
