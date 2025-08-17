Shader "Particles/Normal Visualizer (URP)"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _WingsMax("_WingsMax", Range(0,.5)) = .1
        _WingsFrequency("WingsFrequency", Range(0,20)) = .1
        _CenterMoveVelocity("_CenterMoveVelocity", Range(0,1)) = .1
        
        _Axis("_Axis", Vector) = (0,0,0,0)
        _Angle("Angle", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes 
            {
                float4 vertex   : POSITION;
                float3 normal   : NORMAL;
                float4 uv       : TEXCOORD0; // -> zw = velocity xy
                float4 velocity : TEXCOORD1; // -> velocity z
                float4 color    : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID

            };

            struct Varyings 
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float4 color        : TEXCOORD2;
                float3 velocity     : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _WingsMax;
            float _WingsFrequency;
            float _CenterMoveVelocity;

            float4 _Axis;
            float _Angle;
            

            // Função de rotação em torno de eixo
            float3 rotate_around_axis(float3 position, float3 axis, float angle)
            {
                axis = normalize(axis);
                float s = sin(angle);
                float c = cos(angle);
                float oc = 1.0 - c;
                
                float3x3 rot = float3x3(
                    oc * axis.x * axis.x + c,          oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
                    oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c,          oc * axis.y * axis.z - axis.x * s,
                    oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c
                );
                
                return mul(rot, position);
            }

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);

                // Normal in world space
                o.normalWS = TransformObjectToWorldNormal(v.normal);
                o.color = v.color;
                o.velocity = normalize(float3(v.uv.zw, v.velocity.x)); // Magnitude da velocidade

                // velocity
                float3 posOS = v.vertex.xyz ;
                posOS = rotate_around_axis(posOS , _Axis, _Angle);
                // posOS += sin(_Time.y * _WingsFrequency) * _WingsMax * o.normalWS.xyz * o.color.r;
                // posOS += v.normal * cos(_Time.y * _WingsFrequency ) * _WingsMax;

                // Calcula a posição do objeto no mundo
                // float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                
                o.positionCS = TransformObjectToHClip(posOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Visualize normals (mapping from -1 to 1 into 0 to 1)
                half3 normalColor = i.normalWS;
                return half4(i.velocity, 1);
            }
            ENDHLSL
        }
    }
}