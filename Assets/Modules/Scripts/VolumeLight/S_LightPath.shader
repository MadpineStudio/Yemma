Shader "Custom/S_LightPath"
{
    Properties
    {
        [HDR][MainColor] _Color("Light Color", Color) = (1, 0.9, 0.7, 0.5)
        [MainTexture] _MainTex("Light Texture", 2D) = "white" {}
        [MainTexture] _NoiseTex("Noise Texture", 2D) = "white" {}
        _Intensity("Intensity", Range(0, 3)) = 1.0
        
        [Header(Ray Properties)]
        _Ray1Frequency("Ray 1 Frequency", Range(10, 50)) = 28.0
        _Ray1Speed("Ray 1 Speed", Range(-2, 2)) = -0.7
        _Ray1Strength("Ray 1 Strength", Range(0, 1)) = 0.3
        
        _Ray2Frequency("Ray 2 Frequency", Range(10, 50)) = 34.0
        _Ray2Speed("Ray 2 Speed", Range(-2, 2)) = 0.1
        _Ray2Strength("Ray 2 Strength", Range(0, 1)) = 0.4
        
        _Ray3Frequency("Ray 3 Frequency", Range(10, 50)) = 16.0
        _Ray3Speed("Ray 3 Speed", Range(-2, 2)) = -0.05
        _Ray3Strength("Ray 3 Strength", Range(0, 1)) = 0.4
        
        _Ray4Frequency("Ray 4 Frequency", Range(50, 100)) = 72.0
        _Ray4Speed("Ray 4 Speed", Range(-2, 2)) = 0.9
        _Ray4Strength("Ray 4 Strength", Range(0, 1)) = 0.1
        
        [Header(Smoke Noise)]
        _NoiseDirection("Noise Direction", Vector) = (0, 1, 0, 0)
        _NoiseSpeed("Noise Speed", Range(0, 5)) = 1.0
        _NoiseScale("Noise Scale", Range(0.1, 10)) = 2.0
        _NoiseIntensity("Noise Intensity", Range(0, 1)) = 0.5
        
        [Header(Color Variation)]
        [Toggle] _EnableColorVariation("Enable Color Variation", Float) = 0
        _ColorVariationScale("Color Variation Scale", Range(0.1, 5)) = 1.0
        _ColorVariationIntensity("Color Variation Intensity", Range(0, 1)) = 0.3
        [HDR]_ColorTint1("Color Tint 1", Color) = (1, 0.8, 0.6, 1)
        [HDR]_ColorTint2("Color Tint 2", Color) = (0.8, 0.9, 1, 1)
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
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
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float _Intensity;
                
                float _Ray1Frequency;
                float _Ray1Speed;
                float _Ray1Strength;
                
                float _Ray2Frequency;
                float _Ray2Speed;
                float _Ray2Strength;
                
                float _Ray3Frequency;
                float _Ray3Speed;
                float _Ray3Strength;
                
                float _Ray4Frequency;
                float _Ray4Speed;
                float _Ray4Strength;
                
                float3 _NoiseDirection;
                float _NoiseSpeed;
                float _NoiseScale;
                float _NoiseIntensity;
                
                float _EnableColorVariation;
                float _ColorVariationScale;
                float _ColorVariationIntensity;
                half4 _ColorTint1;
                half4 _ColorTint2;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            // Function to calculate ray value (converted from GLSL)
            float rayValue(float2 coord, float frequency, float travelRate, float maxStrength)
            {
                // Fade out along borders of fragment
                float nx = 2.0 * (coord.x - 0.5);
                float nx2 = min(1.0, 3.5 - 3.5 * nx * nx);
                float ny = 2.0 * (coord.y - 0.5);
                float ny2 = min(1.0, 3.5 - 3.5 * ny * ny);
                
                float xModifier = 0.5 * (cos(_Time.y * travelRate + coord.x * frequency) + 1.0);
                float yModifier = sin(coord.y);
                return maxStrength * xModifier * yModifier * nx2 * ny2;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample texture
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                // Calculate multiple rays with different parameters
                float ray1 = rayValue(IN.uv, _Ray1Frequency, _Ray1Speed, _Ray1Strength);
                float ray2 = rayValue(IN.uv, _Ray2Frequency, _Ray2Speed, _Ray2Strength);
                float ray3 = rayValue(IN.uv, _Ray3Frequency, _Ray3Speed, _Ray3Strength);
                float ray4 = rayValue(IN.uv, _Ray4Frequency, _Ray4Speed, _Ray4Strength);
                
                // Combine all rays
                float totalRays = ray1 + ray2 + ray3 + ray4;
                
                // Calculate world space noise coordinates
                float3 worldNoisePos = IN.worldPos * _NoiseScale;
                float3 animatedPos = worldNoisePos + _NoiseDirection * _Time.y * _NoiseSpeed;
                
                // Sample noise texture using XY world coordinates
                float2 noiseUV = animatedPos.xy;
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;
                
                // Apply noise to rays (simulate smoke)
                float smokeEffect = lerp(1.0, noise, _NoiseIntensity);
                totalRays *= smokeEffect;
                
                // Calculate color variation in local space
                half3 baseColor = _Color.rgb;
                if (_EnableColorVariation > 0.5)
                {
                    // Use static local UV coordinates for color variation (no time animation)
                    float2 colorVariationUV = IN.uv * _ColorVariationScale;
                    
                    // Create static color variation pattern using sin/cos waves
                    float colorPattern1 = sin(colorVariationUV.x * 6.28);
                    float colorPattern2 = cos(colorVariationUV.y * 4.0);
                    float combinedPattern = (colorPattern1 + colorPattern2) * 0.5;
                    
                    // Normalize pattern to 0-1 range
                    combinedPattern = combinedPattern * 0.5 + 0.5;
                    
                    // Lerp between two color tints
                    half3 colorVariation = lerp(_ColorTint1.rgb, _ColorTint2.rgb, combinedPattern);
                    baseColor = lerp(baseColor, baseColor * colorVariation, _ColorVariationIntensity);
                }
                
                // Apply texture and color
                half3 finalColor = tex.rgb * baseColor * _Intensity * totalRays;
                float finalAlpha = tex.a * _Color.a * totalRays;
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}
