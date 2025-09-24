Shader "Universal Render Pipeline/Custom/AnalyticEyeLocal"
{
    Properties
    {
        _SphereOffset ("Sphere Offset", Vector) = (0,0,0,0)
        _SphereRadius ("Sphere Radius", Float) = 1.0
        _RotationEuler ("Rotation Euler", Vector) = (0,0,0,0)
        _ProjectionType ("Projection Type", Int) = 0
        _InvertSphere ("Invert Sphere (Concave)", Range(0,1)) = 0
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        
        [Header(Layer 1)]
        _Layer1Texture ("Layer 1 Texture", 2D) = "white" {}
        _Layer1Scale ("Layer 1 Scale", Vector) = (1,1,1,0)
        _Layer1Offset ("Layer 1 Offset", Vector) = (0,0,0,0)
        _Layer1Blend ("Layer 1 Blend Mode", Int) = 0
        _Layer1Opacity ("Layer 1 Opacity", Range(0,1)) = 1.0
        _Layer1Radius ("Layer 1 Radius", Float) = 1.0
        _Layer1SphereScale ("Layer 1 Sphere Scale", Vector) = (1,1,1,0)
        _Layer1SphereOffset ("Layer 1 Sphere Position", Vector) = (0,0,0,0)
        _Layer1Invert ("Layer 1 Invert", Range(0,1)) = 0
        _Layer1FresnelPower ("Layer 1 Fresnel Power", Float) = 1.0
        _Layer1FresnelIntensity ("Layer 1 Fresnel Intensity", Range(0,1)) = 0.0
        
        [Header(Layer 2)]
        _Layer2Texture ("Layer 2 Texture", 2D) = "white" {}
        _Layer2Scale ("Layer 2 Scale", Vector) = (1,1,1,0)
        _Layer2Offset ("Layer 2 Offset", Vector) = (0,0,0,0)
        _Layer2Blend ("Layer 2 Blend Mode", Int) = 0
        _Layer2Opacity ("Layer 2 Opacity", Range(0,1)) = 1.0
        _Layer2Radius ("Layer 2 Radius", Float) = 0.9
        _Layer2SphereScale ("Layer 2 Sphere Scale", Vector) = (1,1,1,0)
        _Layer2SphereOffset ("Layer 2 Sphere Position", Vector) = (0,0,0,0)
        _Layer2Invert ("Layer 2 Invert", Range(0,1)) = 0
        _Layer2FresnelPower ("Layer 2 Fresnel Power", Float) = 1.0
        _Layer2FresnelIntensity ("Layer 2 Fresnel Intensity", Range(0,1)) = 0.0
        
        [Header(Layer 3)]
        _Layer3Texture ("Layer 3 Texture", 2D) = "white" {}
        _Layer3Scale ("Layer 3 Scale", Vector) = (1,1,1,0)
        _Layer3Offset ("Layer 3 Offset", Vector) = (0,0,0,0)
        _Layer3Blend ("Layer 3 Blend Mode", Int) = 0
        _Layer3Opacity ("Layer 3 Opacity", Range(0,1)) = 1.0
        _Layer3Radius ("Layer 3 Radius", Float) = 0.8
        _Layer3SphereScale ("Layer 3 Sphere Scale", Vector) = (1,1,1,0)
        _Layer3SphereOffset ("Layer 3 Sphere Position", Vector) = (0,0,0,0)
        _Layer3Invert ("Layer 3 Invert", Range(0,1)) = 0
        _Layer3FresnelPower ("Layer 3 Fresnel Power", Float) = 1.0
        _Layer3FresnelIntensity ("Layer 3 Fresnel Intensity", Range(0,1)) = 0.0
    }

    CustomEditor "AnalyticEyeEditor"

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float3 _SphereOffset;
                float  _SphereRadius;
                float4 _RotationEuler;
                int    _ProjectionType;
                float  _InvertSphere;
                float4 _BackgroundColor;
                
                // Layer 1
                float4 _Layer1Texture_ST;
                float4 _Layer1Scale;
                float4 _Layer1Offset;
                int    _Layer1Blend;
                float  _Layer1Opacity;
                float  _Layer1Radius;
                float4 _Layer1SphereScale;
                float4 _Layer1SphereOffset;
                float  _Layer1Invert;
                float  _Layer1FresnelPower;
                float  _Layer1FresnelIntensity;
                
                // Layer 2
                float4 _Layer2Texture_ST;
                float4 _Layer2Scale;
                float4 _Layer2Offset;
                int    _Layer2Blend;
                float  _Layer2Opacity;
                float  _Layer2Radius;
                float4 _Layer2SphereScale;
                float4 _Layer2SphereOffset;
                float  _Layer2Invert;
                float  _Layer2FresnelPower;
                float  _Layer2FresnelIntensity;
                
                // Layer 3
                float4 _Layer3Texture_ST;
                float4 _Layer3Scale;
                float4 _Layer3Offset;
                int    _Layer3Blend;
                float  _Layer3Opacity;
                float  _Layer3Radius;
                float4 _Layer3SphereScale;
                float4 _Layer3SphereOffset;
                float  _Layer3Invert;
                float  _Layer3FresnelPower;
                float  _Layer3FresnelIntensity;
            CBUFFER_END

            TEXTURE2D(_Layer1Texture); SAMPLER(sampler_Layer1Texture);
            TEXTURE2D(_Layer2Texture); SAMPLER(sampler_Layer2Texture);
            TEXTURE2D(_Layer3Texture); SAMPLER(sampler_Layer3Texture);

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 rayOriginOS : TEXCOORD0;
                float3 rayDirOS    : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(posWS);

                // câmera em espaço do objeto
                float3 camPosOS = TransformWorldToObject(GetCameraPositionWS());

                // origem = câmera em espaço local
                OUT.rayOriginOS = camPosOS;

                // direção = posição do vértice em OS - câmera OS
                OUT.rayDirOS = normalize(IN.positionOS.xyz - camPosOS);
                
                // Dados para Fresnel
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - posWS);
                
                return OUT;
            }

            float2 RaySphereHit(float3 ro, float3 rd, float3 center, float radius)
            {
                float3 oc = ro - center;
                float b = dot(oc, rd);
                float c = dot(oc, oc) - radius*radius;
                float h = b*b - c;
                if(h < 0) return float2(-1, -1);
                
                float sqrtH = sqrt(h);
                float t1 = -b - sqrtH; // primeira intersecção (entrada)
                float t2 = -b + sqrtH; // segunda intersecção (saída)
                
                return float2(t1, t2);
            }

            // Projeção esférica (spherical mapping)
            float2 SphereUV(float3 normal)
            {
                float u = atan2(normal.x, normal.z) / (2.0 * 3.14159) + 0.5;
                float v = acos(normal.y) / 3.14159;
                return float2(u, v);
            }

            // Projeção planar frontal (como olhar para uma tela)
            float2 PlanarUV(float3 hitPoint, float3 sphereCenter)
            {
                float3 localPos = hitPoint - sphereCenter;
                // Projeta no plano XY, normaliza pelo raio
                return (localPos.xy / _SphereRadius) * 0.5 + 0.5;
            }

            // Projeção cilíndrica
            float2 CylindricalUV(float3 normal)
            {
                float u = atan2(normal.x, normal.z) / (2.0 * 3.14159) + 0.5;
                float v = normal.y * 0.5 + 0.5;
                return float2(u, v);
            }

            // Projeção radial (iris-like)
            float2 RadialUV(float3 hitPoint, float3 sphereCenter)
            {
                float3 localPos = hitPoint - sphereCenter;
                float2 planar = localPos.xy;
                float distance = length(planar);
                float angle = atan2(planar.y, planar.x);
                
                float u = angle / (2.0 * 3.14159) + 0.5;
                float v = distance / _SphereRadius;
                return float2(u, v);
            }

            // Blend modes
            float3 BlendNormal(float3 base, float3 blend) { return blend; }
            float3 BlendMultiply(float3 base, float3 blend) { return base * blend; }
            float3 BlendScreen(float3 base, float3 blend) { return 1.0 - (1.0 - base) * (1.0 - blend); }
            float3 BlendOverlay(float3 base, float3 blend) { 
                return base < 0.5 ? 2.0 * base * blend : 1.0 - 2.0 * (1.0 - base) * (1.0 - blend); 
            }
            float3 BlendAdd(float3 base, float3 blend) { return base + blend; }

            float3 ApplyBlendMode(float3 base, float3 blend, int mode)
            {
                switch(mode) {
                    case 0: return BlendNormal(base, blend);
                    case 1: return BlendMultiply(base, blend);
                    case 2: return BlendScreen(base, blend);
                    case 3: return BlendOverlay(base, blend);
                    case 4: return BlendAdd(base, blend);
                    default: return BlendNormal(base, blend);
                }
            }

            // Parallax simples e otimizado
            float2 ApplyParallax(float2 uv, float3 viewDirTS, float parallaxAmount)
            {
                if(abs(parallaxAmount) < 0.001) return uv;
                
                float vz = max(abs(viewDirTS.z), 1e-3);
                float2 parallaxOffset = (viewDirTS.xy / vz) * parallaxAmount;
                return uv + parallaxOffset;
            }

            // Matriz de rotação Euler
            float3x3 CreateRotationMatrix(float3 eulerAngles)
            {
                float3 rad = radians(eulerAngles);
                float cx = cos(rad.x), sx = sin(rad.x);
                float cy = cos(rad.y), sy = sin(rad.y);
                float cz = cos(rad.z), sz = sin(rad.z);
                
                return float3x3(
                    cy*cz, -cy*sz, sy,
                    sx*sy*cz + cx*sz, -sx*sy*sz + cx*cz, -sx*cy,
                    -cx*sy*cz + sx*sz, cx*sy*sz + sx*cz, cx*cy
                );
            }

            // Processar layer individual com esfera própria
            float4 ProcessLayer(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius, 
                              float layerInvert, int projType, TEXTURE2D_PARAM(layerTex, layerSampler),
                              float4 layerScale, float4 layerOffset, float fresnelPower, float fresnelIntensity,
                              float3 normalWS, float3 viewDirWS, float4 sphereScale, float4 sphereOffset)
            {
                // Aplicar posição individual da esfera
                float3 layerSphereCenter = sphereCenter + sphereOffset.xyz;
                
                // Criar matriz de rotação
                float3x3 rotMatrix = CreateRotationMatrix(_RotationEuler.xyz);
                
                // Rotacionar ray origin e direction em relação ao centro da esfera
                float3 rotatedRayOrigin = mul(rotMatrix, rayOrigin - layerSphereCenter) + layerSphereCenter;
                float3 rotatedRayDir = mul(rotMatrix, rayDir);
                
                // Aplicar escala apenas no espaço do raio, mantendo o centro fixo
                float3 scaledRayOrigin = (rotatedRayOrigin - layerSphereCenter) / sphereScale.xyz + layerSphereCenter;
                float3 scaledRayDir = normalize(rotatedRayDir / sphereScale.xyz);
                // Centro com offset aplicado
                float3 scaledSphereCenter = layerSphereCenter;
                float scaledRadius = sphereRadius;
                
                float2 intersections = RaySphereHit(scaledRayOrigin, scaledRayDir, scaledSphereCenter, scaledRadius);
                
                float t = layerInvert > 0.5 ? intersections.y : intersections.x;
                if(t < 0 || (layerInvert > 0.5 && intersections.x < 0)) 
                    return float4(0,0,0,0);

                float3 hit = scaledRayOrigin + scaledRayDir * t;
                float3 n = normalize(hit - scaledSphereCenter);
                
                if(layerInvert > 0.5) {
                    n = -n;
                }

                float2 uv;
                if(projType == 0) {
                    uv = SphereUV(n);
                } else if(projType == 1) {
                    uv = PlanarUV(hit, sphereCenter);
                } else if(projType == 2) {
                    uv = CylindricalUV(n);
                } else {
                    uv = RadialUV(hit, sphereCenter);
                }

                // Scale from center with 3D scaling (X,Y separate)
                uv = (uv - 0.5) * layerScale.xy + 0.5 + layerOffset.xy;
                
                float4 color = SAMPLE_TEXTURE2D(layerTex, layerSampler, uv);
                
                // Apply Fresnel effect if enabled
                if(fresnelIntensity > 0.0) {
                    float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), fresnelPower);
                    color.a *= lerp(1.0, fresnel, fresnelIntensity);
                }
                
                return color;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 finalColor = _BackgroundColor.rgb;

                // Layer 1
                float4 layer1Color = ProcessLayer(IN.rayOriginOS, IN.rayDirOS, _SphereOffset, _Layer1Radius,
                                                _Layer1Invert, _ProjectionType, TEXTURE2D_ARGS(_Layer1Texture, sampler_Layer1Texture),
                                                _Layer1Scale, _Layer1Offset, _Layer1FresnelPower, _Layer1FresnelIntensity,
                                                IN.normalWS, IN.viewDirWS, _Layer1SphereScale, _Layer1SphereOffset);
                if(layer1Color.a > 0) {
                    finalColor = lerp(finalColor, layer1Color.rgb, _Layer1Opacity);
                }

                // Layer 2
                float4 layer2Color = ProcessLayer(IN.rayOriginOS, IN.rayDirOS, _SphereOffset, _Layer2Radius,
                                                _Layer2Invert, _ProjectionType, TEXTURE2D_ARGS(_Layer2Texture, sampler_Layer2Texture),
                                                _Layer2Scale, _Layer2Offset, _Layer2FresnelPower, _Layer2FresnelIntensity,
                                                IN.normalWS, IN.viewDirWS, _Layer2SphereScale, _Layer2SphereOffset);
                if(layer2Color.a > 0) {
                    float3 blended2 = ApplyBlendMode(finalColor, layer2Color.rgb, _Layer2Blend);
                    finalColor = lerp(finalColor, blended2, _Layer2Opacity);
                }

                // Layer 3
                float4 layer3Color = ProcessLayer(IN.rayOriginOS, IN.rayDirOS, _SphereOffset, _Layer3Radius,
                                                _Layer3Invert, _ProjectionType, TEXTURE2D_ARGS(_Layer3Texture, sampler_Layer3Texture),
                                                _Layer3Scale, _Layer3Offset, _Layer3FresnelPower, _Layer3FresnelIntensity,
                                                IN.normalWS, IN.viewDirWS, _Layer3SphereScale, _Layer3SphereOffset);
                if(layer3Color.a > 0) {
                    float3 blended3 = ApplyBlendMode(finalColor, layer3Color.rgb, _Layer3Blend);
                    finalColor = lerp(finalColor, blended3, _Layer3Opacity);
                }

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
