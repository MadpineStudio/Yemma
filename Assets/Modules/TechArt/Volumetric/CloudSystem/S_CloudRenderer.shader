Shader "Tutorial/VolumetricFogBoxVolume"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MaxDistance("Max distance", float) = 100
        _StepSize("Step size", Range(0.1, 20)) = 1
        _DensityMultiplier("Density multiplier", Range(0, 10)) = 1
        _NoiseOffset("NoiseOffset", Float) = .1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _Color;
            float _MaxDistance;
            float _DensityMultiplier;
            float _StepSize;
            float _NoiseOffset;
            
            struct CloudAreaData
            {
                float3 bounds;
                float3 position;
            };
            
            StructuredBuffer<CloudAreaData> _CloudBuffer;
            
            bool is_inside_box(float3 position, CloudAreaData cloud_area_data)
            {
                // all() -> se todos os components atenderem a condicao
                return all(position >= -cloud_area_data.bounds * .5) && all(position <= cloud_area_data.bounds * .5);
            }

            float get_density(float3 worldPos, CloudAreaData cloud_area_data)
            {
                return is_inside_box(worldPos, cloud_area_data) ? _DensityMultiplier : 0;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float depth = SampleSceneDepth(IN.texcoord);
                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);

                float3 entryPoint = _WorldSpaceCameraPos;
                float3 viewDir = worldPos - _WorldSpaceCameraPos;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;
                float distLimit = min(viewLength, _MaxDistance);
                float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x))) * _NoiseOffset;
                float transmittance = 1;


                while(distTravelled < distLimit)
                {
                    float3 rayPos = -_CloudBuffer[0].position + entryPoint + rayDir * distTravelled;
                    float density = get_density(rayPos, _CloudBuffer[0]);
                    if (density > 0)
                    {
                        transmittance *= exp(-density * _StepSize);
                    }
                    distTravelled += _StepSize;
                }
                return lerp(col, _Color, 1.0 - saturate(transmittance));
            }
            ENDHLSL
        }
    }
}