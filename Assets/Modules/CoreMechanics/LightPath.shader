Shader "Custom/LightPath"
{
    Properties
    {
        [HDR] _BaseColor("Base Color", Color) = (1, 1, 1, 0.5)
        _Intensity("Intensity", Float) = 1.0
        _Tex("Texture", 2D) = "white" {}
        _Velocity("Velocity", Float) = 1.0
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

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
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Intensity;
                sampler2D _Tex;
                float _Velocity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 center = IN.uv - 0.5;
                float distance = length(center) * 2.0;
                float falloff = 1.0 - distance;
                falloff = max(0.0, falloff);
                
                half3 color = _BaseColor.rgb;
                half alpha = falloff * _BaseColor.a * _Intensity;
                float gradient = tex2D(_Tex, IN.uv * float2(3,1) - float2(_Time.y * _Velocity, 0)).r;
                gradient += tex2D(_Tex, IN.uv * float2(3, 1) - float2(_Time.y * .5 * _Velocity, 0)).r;
                gradient += tex2D(_Tex, (float2(0,1.) - IN.uv) * float2(3, 1) - float2(_Time.y * .7 * _Velocity, 0)).r;

                return half4(_BaseColor.rgb, gradient.x * 1.);
            }
            ENDHLSL
        }
    }
}
