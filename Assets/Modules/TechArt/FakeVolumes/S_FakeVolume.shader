Shader "Custom/S_FakeVolume"
{
    Properties
    {
        [HDR][MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Header(MultiChannel Noise)]
        _NoiseTexture("Multi-Channel Noise (RGBA)", 2D) = "white" {}
        [Header(Alpha Mask)]
        _AlphaMaskTexture("Alpha Mask Texture", 2D) = "white" {}
        _AlphaMaskZoom("Alpha Mask Zoom", Float) = 1.0
        _AlphaMaskOffset("Alpha Mask Offset", Vector) = (0, 0, 0, 0)
        _AlphaMaskIntensity("Alpha Mask Intensity", Range(0, 2)) = 1.0
        [Header(Second Alpha Mask Channel G)]
        _SecondAlphaMaskZoom("Second Alpha Mask Zoom",  Range(0, 1)) = 1.0
        _SecondAlphaMaskOffset("Second Alpha Mask Offset", Vector) = (0, 0, 0, 0)
        _SecondAlphaMaskIntensity("Second Alpha Mask Intensity", Range(0, 10)) = 1.0
        [Header(Channel R  Large Details)]
        _NoiseR_Scale("R Scale", Range(0.1, 10)) = 1
        _NoiseR_Intensity("R Intensity", Range(0, 2)) = 1
        _NoiseR_Speed("R Speed", Range(0, 5)) = 1
        [Header(Channel G  Medium Details)]
        _NoiseG_Scale("G Scale", Range(0.1, 10)) = 2
        _NoiseG_Intensity("G Intensity", Range(0, 2)) = 0.8
        _NoiseG_Speed("G Speed", Range(0, 5)) = 1.5
        [Header(Channel B  Small Details)]
        _NoiseB_Scale("B Scale", Range(0.1, 10)) = 4
        _NoiseB_Intensity("B Intensity", Range(0, 2)) = 0.6
        _NoiseB_Speed("B Speed", Range(0, 5)) = 2
        [Header(Channel A  Micro Details)]
        _NoiseA_Scale("A Scale", Range(0.1, 10)) = 8
        _NoiseA_Intensity("A Intensity", Range(0, 2)) = 0.4
        _NoiseA_Speed("A Speed", Range(0, 5)) = 3
        [Header(Main Gradient)]
        _MainColorA("Main Color A", Color) = (1, 1, 1, 1)
        _MainColorB("Main Color B", Color) = (0.6, 0.7, 0.9, 1)
        _GradientPower("Gradient Power", Range(0.1, 5)) = 1
        _GradientMin("Gradient Min", Range(0, 1)) = 0
        _GradientMax("Gradient Max", Range(0, 1)) = 1
        _Alpha("Overall Alpha", Range(0, 1)) = 0.5
        _FadeHeight("Fade Height", Float) = 2.0
        _FadeSharpness("Fade Sharpness", Range(0.1, 5)) = 1.0
        _VerticalOffset("Vertical Offset", Float) = 0.0
        _DepthFadeDistance("Depth Fade Distance", Float) = 1.0
        _DepthDensity("Depth Density", Range(0, 2)) = 1.0
        _MaxDepthDistance("Max Depth Distance", Float) = 10.0
        [Header(Movement)]
        _WindDirection("Wind Direction", Vector) = (1, 0, 0, 0)
        _WindSpeed("Wind Speed", Range(0, 5)) = 1.0
        _MovementScale("Movement Scale", Range(0.1, 3)) = 1.0
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float height : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float4 movementOffsetR_G : TEXCOORD4; // R channel movement in xy, G channel in zw
                float4 movementOffsetB_A : TEXCOORD5; // B channel movement in xy, A channel in zw
            };

            TEXTURE2D(_NoiseTexture);
            SAMPLER(sampler_NoiseTexture);
            TEXTURE2D(_AlphaMaskTexture);
            SAMPLER(sampler_AlphaMaskTexture);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _NoiseTexture_ST;
                float4 _AlphaMaskTexture_ST;
                // Channel R
                half _NoiseR_Scale;
                half _NoiseR_Intensity;
                half _NoiseR_Speed;
                // Channel G
                half _NoiseG_Scale;
                half _NoiseG_Intensity;
                half _NoiseG_Speed;
                // Channel B
                half _NoiseB_Scale;
                half _NoiseB_Intensity;
                half _NoiseB_Speed;
                // Channel A
                half _NoiseA_Scale;
                half _NoiseA_Intensity;
                half _NoiseA_Speed;
                // Colors
                half4 _MainColorA;
                half4 _MainColorB;
                half _GradientPower;
                half _GradientMin;
                half _GradientMax;
                // Alpha Mask
                float _AlphaMaskZoom;
                float4 _AlphaMaskOffset;
                half _AlphaMaskIntensity;
                // Second Alpha Mask
                float _SecondAlphaMaskZoom;
                float4 _SecondAlphaMaskOffset;
                half _SecondAlphaMaskIntensity;
                // Other properties
                half _Alpha;
                float _FadeHeight;
                half _FadeSharpness;
                float _VerticalOffset;
                float _DepthFadeDistance;
                half _DepthDensity;
                float _MaxDepthDistance;
                float4 _WindDirection;
                half _WindSpeed;
                half _MovementScale;
            CBUFFER_END

            // Function to calculate depth-based density
            float GetDepthFade(float4 screenPos, float3 worldPos)
            {
                // Get screen space UV
                float2 screenUV = screenPos.xy / screenPos.w;
                
                // Sample scene depth using URP function
                float sceneDepth = SampleSceneDepth(screenUV);
                
                // Convert to linear depth
                float sceneDepthLinear = LinearEyeDepth(sceneDepth, _ZBufferParams);
                
                // Get current fragment depth
                float fragDepth = LinearEyeDepth(screenPos.z / screenPos.w, _ZBufferParams);
                
                // Calculate depth difference (how much depth we're seeing "through")
                float depthDifference = sceneDepthLinear - fragDepth;
                
                // Apply depth fade curve
                float depthFade = saturate(depthDifference / _DepthFadeDistance);
                
                // Apply density multiplier
                return pow(depthFade, _DepthDensity);
            }
            
            // Function to remap gradient value with power and min/max controls
            half RemapGradient(half value, half power, half minVal, half maxVal)
            {
                // Apply power curve
                half poweredValue = pow(saturate(value), power);
                
                // Remap from [0,1] to [minVal, maxVal]
                return lerp(minVal, maxVal, poweredValue);
            }
            
            // Function to zoom UV from center with offset
            float2 ZoomUVFromCenterWithOffset(float2 uv, float zoom, float2 offset)
            {
                // Move UV to center (0,0)
                float2 centeredUV = uv - 0.5;
                
                // Apply zoom (division for zoom in, multiplication for zoom out)
                centeredUV = centeredUV / zoom;
                
                // Apply offset
                centeredUV += offset;
                
                // Move back to original space
                return centeredUV + 0.5;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // Transform to world space first
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _NoiseTexture);
                
                // Calculate height relative to object's pivot + offset
                OUT.height = OUT.positionWS.y - _VerticalOffset;
                
                // Calculate screen position for depth sampling
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                
                // Calculate independent movement offsets for each channel
                float2 windDir = normalize(_WindDirection.xy);
                float baseTime = _Time.y * _WindSpeed * _MovementScale;
                
                // Each channel moves at different speeds
                OUT.movementOffsetR_G.xy = windDir * (baseTime * _NoiseR_Speed);
                OUT.movementOffsetR_G.zw = windDir * (baseTime * _NoiseG_Speed);
                OUT.movementOffsetB_A.xy = windDir * (baseTime * _NoiseB_Speed);
                OUT.movementOffsetB_A.zw = windDir * (baseTime * _NoiseA_Speed);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample each channel of noise texture with independent movement and scale
                float2 noiseUV_R = (IN.uv * _NoiseR_Scale) + IN.movementOffsetR_G.xy;
                float2 noiseUV_G = (IN.uv * _NoiseG_Scale) + IN.movementOffsetR_G.zw;
                float2 noiseUV_B = (IN.uv * _NoiseB_Scale) + IN.movementOffsetB_A.xy;
                float2 noiseUV_A = (IN.uv * _NoiseA_Scale) + IN.movementOffsetB_A.zw;
                
                // Sample all channels
                half4 noiseValues = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV_R);
                half noiseG = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV_G).g;
                half noiseB = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV_B).b;
                half noiseA = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV_A).a;
                
                // Apply individual intensities (additive combination)
                half noiseR_alpha = noiseValues.r * _NoiseR_Intensity;
                half noiseG_alpha = noiseG * _NoiseG_Intensity;
                half noiseB_alpha = noiseB * _NoiseB_Intensity;
                half noiseA_alpha = noiseA * _NoiseA_Intensity;
                
                // Combine noise channels additively for alpha and gradient
                half combinedNoiseAlpha = saturate(noiseR_alpha + noiseG_alpha + noiseB_alpha + noiseA_alpha);
                
                // Remap the gradient value with power and min/max controls
                half gradientValue = RemapGradient(combinedNoiseAlpha, _GradientPower, _GradientMin, _GradientMax);
                
                // Use remapped value for gradient interpolation
                half3 mainGradientColor = lerp(_MainColorA.rgb, _MainColorB.rgb, gradientValue);
                
                // Calculate vertical fade based on height
                float heightFactor = saturate(IN.height / _FadeHeight);
                float verticalFade = pow(1.0 - heightFactor, _FadeSharpness);
                
                // Calculate depth-based density
                float depthDensity = GetDepthFade(IN.screenPos, IN.positionWS);
                
                // Sample alpha mask with zoom and offset from separate texture
                float2 alphaMaskUV = ZoomUVFromCenterWithOffset(IN.uv, _AlphaMaskZoom, _AlphaMaskOffset.xy);
                half alphaMask = SAMPLE_TEXTURE2D(_AlphaMaskTexture, sampler_AlphaMaskTexture, alphaMaskUV).r;
                alphaMask = pow(alphaMask, _AlphaMaskIntensity);
                
                // Sample second alpha mask using channel G with independent controls
                float2 secondAlphaMaskUV = ZoomUVFromCenterWithOffset(IN.uv, _SecondAlphaMaskZoom, _SecondAlphaMaskOffset.xy);
                half secondAlphaMask = SAMPLE_TEXTURE2D(_AlphaMaskTexture, sampler_AlphaMaskTexture, secondAlphaMaskUV).g;
                secondAlphaMask = pow(secondAlphaMask, _SecondAlphaMaskIntensity);
                
                // Combine base color with gradient
                half3 finalColor = _BaseColor.rgb * mainGradientColor;
                
                // Combine all factors for final alpha, including both alpha masks
                half finalAlpha = _Alpha * combinedNoiseAlpha * verticalFade * depthDensity * alphaMask * secondAlphaMask;
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}
