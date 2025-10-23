Shader "Custom/S_skybox"
{
    Properties
    {
        [Header(Layered Skybox)]
        [MainTexture] _LayerTexture("Layer Texture (RGBA)", 2D) = "black" {}
        
        [Header(Layer 01  R Channel)]
        [HDR] _Layer01Color("Layer 01 Color", Color) = (1, 1, 1, 1)
        _Layer01Rotation("Layer 01 Rotation", Range(0, 360)) = 0
        _Layer01Tiling("Layer 01 Tiling", Vector) = (1, 1, 0, 0)
        
        [Header(Layer 02  G Channel)]
        [HDR] _Layer02Color("Layer 02 Color", Color) = (1, 1, 1, 1)
        _Layer02Rotation("Layer 02 Rotation", Range(0, 360)) = 0
        _Layer02Tiling("Layer 02 Tiling", Vector) = (1, 1, 0, 0)
        
        [Header(Layer 03  B Channel)]
        [HDR] _Layer03Color("Layer 03 Color", Color) = (1, 1, 1, 1)
        _Layer03Rotation("Layer 03 Rotation", Range(0, 360)) = 0
        _Layer03Tiling("Layer 03 Tiling", Vector) = (1, 1, 0, 0)
        
        [Header(Layer 04  A Channel)]
        [HDR] _Layer04Color("Layer 04 Color", Color) = (1, 1, 1, 1)
        _Layer04Rotation("Layer 04 Rotation", Range(0, 360)) = 0
        _Layer04Tiling("Layer 04 Tiling", Vector) = (1, 1, 0, 0)
        
        [Header(Gradient)]
        _EnableGradient("Enable Gradient", Range(0, 1)) = 1
        [HDR] _GradientTop("Gradient Top Color", Color) = (0.02, 0.05, 0.15, 1)
        [HDR] _GradientBottom("Gradient Bottom Color", Color) = (0.05, 0.15, 0.35, 1)
        _GradientIntensity("Gradient Intensity", Range(0, 3)) = 1
        _GradientPower("Gradient Power", Range(0.1, 8)) = 2
        _GradientOffset("Gradient Offset", Range(-1, 1)) = 0
        
        [Header(Stars)]
        _EnableStars("Enable Stars", Range(0, 1)) = 1
        _StarDensity("Star Density", Range(0.1, 10)) = 1
        _StarBrightness("Star Brightness", Range(0, 3)) = 0.8
        _StarTwinkle("Star Twinkle", Range(0, 1)) = 0.2
        
        [Header(Layer 05  Planet)]
        _PlanetTexture("Planet Texture", 2D) = "white" {}
        [HDR] _PlanetColor("Planet Color", Color) = (1, 1, 1, 1)
        _PlanetRotation("Planet Rotation", Range(0, 360)) = 0
        _PlanetZoom("Planet Zoom", Float) = 1.0
        _PlanetDistortion("Planet Distortion XY", Vector) = (1, 1, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Background" "Queue" = "Background" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off
    
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldDir : TEXCOORD0;
            };

            TEXTURE2D(_LayerTexture);
            SAMPLER(sampler_LayerTexture);
            TEXTURE2D(_PlanetTexture);
            SAMPLER(sampler_PlanetTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _LayerTexture_ST;
                half4 _Layer01Color;
                half4 _Layer02Color;
                half4 _Layer03Color;
                half4 _Layer04Color;
                float _Layer01Rotation;
                float _Layer02Rotation;
                float _Layer03Rotation;
                float _Layer04Rotation;
                float4 _Layer01Tiling;
                float4 _Layer02Tiling;
                float4 _Layer03Tiling;
                float4 _Layer04Tiling;
                
                // Gradient properties
                float _EnableGradient;
                half4 _GradientTop;
                half4 _GradientBottom;
                float _GradientIntensity;
                float _GradientPower;
                float _GradientOffset;
                
                // Stars properties
                float _EnableStars;
                float _StarDensity;
                float _StarBrightness;
                float _StarTwinkle;
                
                // Planet properties
                float4 _PlanetTexture_ST;
                half4 _PlanetColor;
                float _PlanetRotation;
                float _PlanetZoom;
                float4 _PlanetDistortion;
            CBUFFER_END

            // High quality procedural star generation
            float3 GenerateStars(float3 dir, float density, float brightness, float twinkle)
            {
                // Convert direction to stable UV coordinates (fixed in world space)
                float2 sphereUV = float2(
                    atan2(dir.x, dir.z) / (2.0 * PI) + 0.5,
                    acos(-dir.y) / PI
                );
                
                float3 starColor = float3(0, 0, 0);
                
                // High-resolution star field with aspect correction
                float2 starField = sphereUV * density * float2(64.0, 32.0);
                
                // Layer 1 - Bright stars
                float2 cell1 = floor(starField);
                float2 localFrac1 = frac(starField);
                
                // Hash escalar para decidir presença da estrela (usa X e Y)
                float starPresence1 = frac(sin(dot(cell1, float2(127.1, 311.7))) * 43758.5453);
                
                // Only create star if hash value is high enough (controls density)
                if(starPresence1 > 0.95)
                {
                    // Hash 2D separado para posição dentro da célula
                    float2 starPos1 = frac(sin(cell1 * float2(269.5, 183.3)) * float2(43758.5453, 94673.2467));
                    
                    // Distance from fragment to star center within cell
                    float2 starOffset = starPos1 - localFrac1;
                    float starDist = length(starOffset);
                    
                    // Create sharp, small star with smooth falloff
                    float starMask = 1.0 - smoothstep(0.0, 0.05, starDist);
                    starMask = pow(starMask, 4.0);
                    
                    // Star color variation
                    float colorSeed = frac(sin(dot(cell1, float2(12.9898, 78.233))) * 43758.5453);
                    float3 starTint = lerp(float3(0.8, 0.9, 1.0), float3(1.0, 0.9, 0.7), colorSeed);
                    
                    // Brightness variation
                    float brightSeed = frac(sin(dot(cell1, float2(93.9898, 67.233))) * 23758.5453);
                    float starBright = lerp(0.5, 2.0, pow(brightSeed, 2.0));
                    
                    starColor += starMask * starTint * starBright * brightness;
                }
                
                // Layer 2 - Medium stars (different scale)
                float2 starField2 = sphereUV * density * float2(32.0, 16.0);
                float2 cell2 = floor(starField2);
                float2 localFrac2 = frac(starField2);
                
                // Hash escalar para presença (usa X e Y)
                float starPresence2 = frac(sin(dot(cell2, float2(169.5, 412.8))) * 73421.8291);
                
                if(starPresence2 > 0.92)
                {
                    // Hash 2D separado para posição dentro da célula
                    float2 starPos2 = frac(sin(cell2 * float2(374.1, 521.7)) * float2(73421.8291, 15329.4567));
                    
                    float2 starOffset2 = starPos2 - localFrac2;
                    float starDist2 = length(starOffset2);
                    
                    float starMask2 = 1.0 - smoothstep(0.0, 0.03, starDist2);
                    starMask2 = pow(starMask2, 3.0);
                    
                    float colorSeed2 = frac(sin(dot(cell2, float2(41.9898, 29.233))) * 63758.5453);
                    float3 starTint2 = lerp(float3(0.7, 0.8, 1.0), float3(1.0, 0.8, 0.6), colorSeed2);
                    
                    float brightSeed2 = frac(sin(dot(cell2, float2(78.1234, 45.678))) * 33421.9876);
                    float starBright2 = lerp(0.3, 1.0, pow(brightSeed2, 2.0));
                    
                    starColor += starMask2 * starTint2 * starBright2 * brightness * 0.8;
                }
                
                // Layer 3 - Faint background stars
                float2 starField3 = sphereUV * density * float2(16.0, 8.0);
                float2 cell3 = floor(starField3);
                float2 localFrac3 = frac(starField3);
                
                // Hash escalar para presença (usa X e Y)
                float starPresence3 = frac(sin(dot(cell3, float2(214.7, 651.2))) * 92847.3156);
                
                if(starPresence3 > 0.88)
                {
                    // Hash 2D separado para posição dentro da célula
                    float2 starPos3 = frac(sin(cell3 * float2(481.9, 762.3)) * float2(92847.3156, 67234.8901));
                    
                    float2 starOffset3 = starPos3 - localFrac3;
                    float starDist3 = length(starOffset3);
                    
                    float starMask3 = 1.0 - smoothstep(0.0, 0.02, starDist3);
                    starMask3 = pow(starMask3, 2.0);
                    
                    starColor += starMask3 * float3(0.8, 0.9, 1.0) * brightness * 0.6;
                }
                
                // Subtle twinkle effect
                if(twinkle > 0.0)
                {
                    float twinkleTime = _Time.y * 0.5;
                    float twinkleSeed = frac(sin(dot(floor(starField), float2(12.9898, 78.233))) * 43758.5453);
                    float twinklePhase = sin(twinkleTime * 2.0 + twinkleSeed * 10.0) * 0.5 + 0.5;
                    float twinkleFactor = lerp(0.8, 1.2, twinklePhase);
                    starColor *= lerp(1.0, twinkleFactor, twinkle * 0.5);
                }
                
                return starColor;
            }

            // Calculate gradient color based on vertical direction
            half3 CalculateGradient(float3 dir)
            {
                // Normalize vertical component (-1 = bottom, +1 = top)
                float verticalFactor = dir.y;
                
                // Apply offset first
                verticalFactor += _GradientOffset;
                
                // Remap from [-1,1] to [0,1] with proper clamping
                verticalFactor = saturate((verticalFactor + 1.0) * 0.5);
                
                // Apply power curve for smooth transitions
                verticalFactor = pow(verticalFactor, _GradientPower);
                
                // Interpolate between bottom and top colors
                half3 gradientColor = lerp(_GradientBottom.rgb, _GradientTop.rgb, verticalFactor);
                
                return gradientColor * _GradientIntensity;
            }

            // Convert 3D direction to equirectangular UV with rotation and tiling
            float2 DirectionToEquirectUV(float3 dir, float rotationDegrees, float4 tiling)
            {
                // Apply Y-axis rotation
                float rad = radians(rotationDegrees);
                float cosRot = cos(rad);
                float sinRot = sin(rad);
                
                float3 rotatedDir;
                rotatedDir.x = dir.x * cosRot - dir.z * sinRot;
                rotatedDir.y = dir.y;
                rotatedDir.z = dir.x * sinRot + dir.z * cosRot;
                
                // Convert to equirectangular coordinates
                float phi = atan2(rotatedDir.z, rotatedDir.x);
                float theta = acos(rotatedDir.y);
                
                float2 uv;
                uv.x = phi / (2.0 * PI) + 0.5;
                uv.y = theta / PI;
                
                // Apply tiling (xy = scale, zw = offset)
                uv = uv * tiling.xy + tiling.zw;
                
                return uv;
            }

            // Convert 3D direction to equirectangular UV with rotation, zoom and distortion for planet
            float2 DirectionToPlanetUV(float3 dir, float rotationDegrees, float zoom, float4 distortion)
            {
                // Apply Y-axis rotation (horizontal rotation) - rotates the entire sky space
                float radY = radians(rotationDegrees);
                float cosRotY = cos(radY);
                float sinRotY = sin(radY);
                
                float3 rotatedDir;
                rotatedDir.x = dir.x * cosRotY - dir.z * sinRotY;
                rotatedDir.y = dir.y;
                rotatedDir.z = dir.x * sinRotY + dir.z * cosRotY;
                
                // Apply X-axis rotation (pitch/height) using distortion.z - rotates the entire sky space
                float radX = radians(distortion.z);
                float cosRotX = cos(radX);
                float sinRotX = sin(radX);
                
                float3 finalDir;
                finalDir.x = rotatedDir.x;
                finalDir.y = rotatedDir.y * cosRotX - rotatedDir.z * sinRotX;
                finalDir.z = rotatedDir.y * sinRotX + rotatedDir.z * cosRotX;
                
                // Convert to equirectangular coordinates
                float phi = atan2(finalDir.z, finalDir.x);
                float theta = acos(clamp(finalDir.y, -1.0, 1.0));
                
                float2 uv;
                uv.x = phi / (2.0 * PI) + 0.5;
                uv.y = theta / PI;
                
                // Apply distortion XY (stretching the texture itself)
                uv *= distortion.xy;
                
                // Apply zoom (scaling the texture around 0.5,0.5 center)
                float2 centeredUV = uv - 0.5;
                centeredUV /= zoom;
                uv = centeredUV + 0.5;
                
                return uv;
            }
            
            // Render planet layer like other layers
            half4 RenderLayerPlanet(float3 dir, float rotationDegrees, float zoom, float4 distortion, half4 color)
            {
                // Get equirectangular UV with planet-specific function
                float2 uv = DirectionToPlanetUV(dir, rotationDegrees, zoom, distortion);
                
                // Sample planet texture
                half4 planetSample = SAMPLE_TEXTURE2D(_PlanetTexture, sampler_PlanetTexture, uv);
                
                return planetSample * color;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                
                // Get world space direction directly from object space position
                // This creates a global skybox that moves with camera rotation
                OUT.worldDir = TransformObjectToWorldDir(IN.positionOS.xyz);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.worldDir);
                
                // Sample all layers with their respective rotations and tiling
                float2 uv1 = DirectionToEquirectUV(dir, _Layer01Rotation, _Layer01Tiling);
                float2 uv2 = DirectionToEquirectUV(dir, _Layer02Rotation, _Layer02Tiling);
                float2 uv3 = DirectionToEquirectUV(dir, _Layer03Rotation, _Layer03Tiling);
                float2 uv4 = DirectionToEquirectUV(dir, _Layer04Rotation, _Layer04Tiling);
                
                half4 layerSample1 = SAMPLE_TEXTURE2D(_LayerTexture, sampler_LayerTexture, uv1);
                half4 layerSample2 = SAMPLE_TEXTURE2D(_LayerTexture, sampler_LayerTexture, uv2);
                half4 layerSample3 = SAMPLE_TEXTURE2D(_LayerTexture, sampler_LayerTexture, uv3);
                half4 layerSample4 = SAMPLE_TEXTURE2D(_LayerTexture, sampler_LayerTexture, uv4);
                
                // Extract masks from respective channels
                half mask1 = layerSample1.r;
                half mask2 = layerSample2.g;
                half mask3 = layerSample3.b;
                half mask4 = layerSample4.a;
                
                // Compute exclusive masks for strict priority: 04 > 03 > 02 > 01
                half rem = 1.0h;
                half e4 = min(mask4, rem);
                rem = max(0.0h, rem - e4);
                half e3 = min(mask3, rem);
                rem = max(0.0h, rem - e3);
                half e2 = min(mask2, rem);
                rem = max(0.0h, rem - e2);
                half e1 = min(mask1, rem);
                
                // Calculate gradient as base color
                half3 gradientColor = CalculateGradient(dir);
                
                // Start with gradient as base if enabled, otherwise black
                half3 finalColor = lerp(half3(0, 0, 0), gradientColor, _EnableGradient);
                
                // Generate stars
                half3 starColor = GenerateStars(dir, _StarDensity, _StarBrightness, _StarTwinkle);
                finalColor += lerp(half3(0, 0, 0), starColor, _EnableStars);
                
                // Apply layers in priority order with HDR colors (additive over gradient and stars)
                finalColor += e1 * _Layer01Color.rgb * _Layer01Color.a;
                finalColor += e2 * _Layer02Color.rgb * _Layer02Color.a;
                finalColor += e3 * _Layer03Color.rgb * _Layer03Color.a;
                finalColor += e4 * _Layer04Color.rgb * _Layer04Color.a;
                
                // Add planet additively (Layer 05)
                half4 planet = RenderLayerPlanet(dir, _PlanetRotation, _PlanetZoom, _PlanetDistortion, _PlanetColor);
                finalColor += planet.rgb * planet.a;
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
