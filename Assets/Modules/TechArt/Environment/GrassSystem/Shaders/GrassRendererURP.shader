Shader "Custom/GrassRendererURP"
{
    Properties
    {
         _GrassScale("_GrassScale", Float) = 1
        _WindScale("_WindScale", Range(.0001, 3)) = .1
        _WindStrength("_WindStrength", Float) = .1
        _WindVelocity("_WindVelocity", Vector) = (0,0,0,0)

        _BaseOffsetY("_BaseOffsetY", Float) = .1
        _Intensity("_Intensity", Float) = .1
        _shadowOcclusion("_shadowOcclusion", Float) = 1
        _shadowOcclusionHeight("_shadowOcclusionHeight", Float) = 0
        
        _ShadowColor("_ShadowColor", Color) = (1,1,1,1)
        _Color("Color", Color) = (1,1,1,1)
        _LightColor("_LightColor", Color) = (0,0,0,0)
        _WindColor("_WindColor", Color) = (0,0,0,0)
        
        _WindTex("WindTex", 2D) = "white" {} 
        _GrassMask("GrassMask", 2D) = "white" {}
        _GrassGroundMask("grass ground Mask", 2D) = "white" {}
        _InputMask ("_InputMask", 2D) = "white" {}
        
        
        _grassScale("_grassScale", Float) = .1
        _grassDisplaceIntensity("_grassDisplaceIntensity", Float) = .1
        _grassOffset("_grassOffset", Vector) = (0,0,0,0)
        
    }

    SubShader
    {
        Tags { 
            "RenderType" = "TransparentCutout"  // Alterado de "Opaque"
            "Queue" = "Transparent"             // Renderiza depois de objetos opacos
            "RenderPipeline" = "UniversalPipeline"
        }
        Cull Off
        LOD 0
        ZWrite On
        ZTest On
        ZClip True

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct DrawVertex {
                float3 positionWS;
                float2 uv;
            };

            struct DrawTriangle {
                float3 normalWS;
                DrawVertex vertices[3];
            };

            StructuredBuffer<DrawTriangle> _DrawTriangles;
            
            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
            float4 positionCS       : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normals      : TEXCOORD1;
                float wind          : TEXCOORD2;
                float3 posWS        : TEXCOORD3;
                float4 shadowCoord  : TEXCOORD4;  // Adicionado para sombras
                float3 displaceGrass : TEXCOORD5;

            };
            // TODO: CONFIRMAR O CBUFFER COM O BATCHER, GPU é depois SRP
            CBUFFER_START(UnityPerMaterial)
            sampler2D _WindTex;
            sampler2D _GrassMask;
            sampler2D _GrassGroundMask;
            sampler2D _InputMask;

            float2 _WindVelocity;
            float _WindStrength;
            float _WindScale;
            float _GrassScale;

            float _BaseOffsetY;
            float _Intensity;
            float _shadowOcclusion;
            float _shadowOcclusionHeight;

            float _grassScale;
            float _grassDisplaceIntensity;
            float2 _grassOffset;


            float3 _ShadowColor;
            float3 _Color;
            float3 _LightColor;
            float3 _WindColor;
            CBUFFER_END


        float3 FixBackfaceNormal(float3 normalWS, float3 viewDirWS)
        {
            return dot(normalWS, viewDirWS) < 0 ? -normalWS : normalWS;
        }
            
            Varyings vert(Attributes input)
            {
                Varyings o;
                int triIndex = input.vertexID / 3;
                int vertIndex = input.vertexID % 3;
                
                DrawTriangle tri = _DrawTriangles[triIndex];
                DrawVertex v = tri.vertices[vertIndex];
                o.uv = v.uv;
                o.posWS = v.positionWS;

                 // Calculate view direction in world space
                float3 viewDirWS = GetWorldSpaceViewDir(v.positionWS);
                o.normals = FixBackfaceNormal(TransformObjectToWorldNormal(tri.normalWS), viewDirWS);
                float wind =  tex2Dlod(_WindTex, float4(o.posWS.xz * _WindScale + _WindVelocity * _Time.y ,0,0));
                wind = pow(wind, _WindStrength);

                // displacement
                o.displaceGrass = tex2Dlod(_InputMask, float4(o.posWS.xz * _grassScale + _grassOffset.xy ,0,0));

                o.wind = wind;
                o.posWS.xz += wind * v.uv.y;
                // o.posWS.xz += o.displaceGrass.xz * ;
                // o.posWS.y -= o.displaceGrass * _grassDisplaceIntensity;
                o.positionCS = TransformObjectToHClip(float4(o.posWS.xyz, 1.));

                // Calculate shadow coordinates
                o.shadowCoord = TransformWorldToShadowCoord(o.posWS);
                
                return o;
            }
            half4 frag(Varyings input) : SV_Target
            {
                half3 texColor = tex2D(_GrassMask, input.uv - float2(0.05, .1));
                float3 ground = tex2D(_GrassGroundMask, input.posWS.xz * _GrassScale);

                clip(texColor.x - .1);

                // Shadow calculation
                Light mainLight = GetMainLight(input.shadowCoord);
                float shadow = mainLight.shadowAttenuation;
                shadow = smoothstep(0.25, 0.75, shadow);

                // Lighting calculation with shadows
                float3 lightDir = mainLight.direction;
                float NdotL = max(saturate(dot(input.normals, lightDir)), 0.0);
                
                // Final color with shadows
                float occlusion = saturate(lerp(pow(input.uv.y - _shadowOcclusionHeight, _shadowOcclusion) , 1.,shadow));

                float windMask = saturate(smoothstep(.01, .3, input.wind.x)) * .2;
                windMask = smoothstep(.01, .5, windMask);
                half3 color = ground ;// * _Intensity + windMask;//lerp( ground * _Intensity + windMask, _Color, input.uv.y);
                // color = lerp(color, _LightColor, NdotL * _BaseOffsetY);
                color = lerp(_ShadowColor, color, occlusion);
                // color = lerp(color, _WindColor, smoothstep(.04,.1, windMask).x);
                // return half4(smoothstep(.0001,.1, windMask).xxx,  1);
                float x =  step(.09, input.wind);   
                return half4(color + x * _Intensity ,  1);
                // return half4(ground,  1);
            }
            ENDHLSL
        }

    // Pass
    //{
    //    Name "ShadowCaster"
    //    Tags { "LightMode" = "ShadowCaster" }
    //
    //    HLSLPROGRAM
    //    #pragma vertex vert
    //    #pragma fragment frag
    //    #pragma multi_compile_instancing
    //    #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
    //
    //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    //
    //    struct Attributes
    //    {
    //        uint vertexID : SV_VertexID;
    //    };
    //
    //    struct Varyings
    //    {
    //        float4 positionCS : SV_POSITION;
    //        float2 uv : TEXCOORD0;
    //    };
    //    struct DrawVertex {
    //                float3 positionWS;
    //                float2 uv;
    //            };
    //
    //            struct DrawTriangle {
    //                float3 normalWS;
    //                DrawVertex vertices[3];
    //            };
    //
    //    StructuredBuffer<DrawTriangle> _DrawTriangles;
    //    
    //    CBUFFER_START(UnityPerMaterial)
    //        sampler2D _WindTex;
    //        sampler2D _GrassMask;
    //        float2 _WindVelocity;
    //        float _WindStrength;
    //        float _WindScale;
    //    CBUFFER_END
    //
    //    Varyings vert(Attributes input)
    //    {
    //        Varyings o;
    //        int triIndex = input.vertexID / 3;
    //        int vertIndex = input.vertexID % 3;
    //        
    //        DrawTriangle tri = _DrawTriangles[triIndex];
    //        DrawVertex v = tri.vertices[vertIndex];
    //        o.uv = v.uv;
    //        
    //        // Apply wind effect (same as in the main pass)
    //        float wind = v.uv.y * tex2Dlod(_WindTex, float4(v.positionWS.xz * _WindScale + _WindVelocity * _Time.y, 1, 1));
    //        wind = pow(wind, _WindStrength);
    //        
    //        // Apply wind displacement to position
    //        float3 positionWS = v.positionWS;
    //        positionWS.xz += wind;
    //        
    //        // Transform position to shadow caster space
    //        o.positionCS = TransformWorldToHClip(positionWS);
    //        
    //        #if _CASTING_PUNCTUAL_LIGHT_SHADOW
    //            // Apply bias for point/spot lights
    //            float3 lightDirectionWS = normalize(_LightPosition.xyz - positionWS);
    //            o.positionCS.z += _LightPosition.w;
    //            o.positionCS.z = min(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    //        #else
    //            // Apply normal bias for directional lights
    //             Light mainLight = GetMainLight();
    //            float3 lightDir = mainLight.direction;
    //            float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, tri.normalWS, lightDir));
    //            #if UNITY_REVERSED_Z
    //                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    //            #else
    //                positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    //            #endif
    //            o.positionCS = positionCS;
    //        #endif
    //        
    //        return o;
    //    }
    //
    //    half4 frag(Varyings input) : SV_Target
    //    {
    //        // Alpha clipping - same as in main pass
    //        half3 texColor = tex2D(_GrassMask, input.uv - float2(0., .1));
    //        clip(texColor.x - 0.1);
    //        
    //        return 0;
    //    }
    //    ENDHLSL
    //}
    }
    FallBack "Universal Render Pipeline/Lit"
}