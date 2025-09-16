Shader "Custom/LightShaft"
{
    Properties
    {
        _ChromaticAmount("Chromatic Amount", Float) = 0.1
        _ChromaticIntensity("Chromatic Intensity", Float) = 1.0
        _MainTex("Main Texture", 2D) = "white" {}
        _CenterColor("Center Color", Color) = (1, 1, 1, 1)
        _EdgeColor("Edge Color", Color) = (0, 0, 0, 1)
        _EdgeIntensity("Edge Intensity", Float) = 1.0
        _GradientPower("Gradient Power", Float) = 1.0
        _StartPoint("Start Point", Vector) = (0, 0, 0, 0)
        _EndPoint("End Point", Vector) = (1, 0, 0, 0)
        _LineLength("Line Length", Float) = 1.0
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
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _ChromaticAmount;
                float _ChromaticIntensity;
                float4 _MainTex_ST;
                half4 _CenterColor;
                half4 _EdgeColor;
                float _EdgeIntensity;
                float _GradientPower;
                float4 _StartPoint;
                float4 _EndPoint;
                float _LineLength;
            CBUFFER_END

            // Função de aberração cromática vertical simples com textura
            float3 ChromaticAberration(float2 uv, float amount, float intensity)
            {
                float2 offsetR = float2(0, 0);
                float2 offsetG = float2(0, amount * 0.5);
                float2 offsetB = float2(0, amount);
                
                float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsetR).r;
                float g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsetG).g;
                float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsetB).b;
                
                // Intensifica a separação dos canais
                float3 chromatic = float3(r, g, b);
                float3 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
                
                return lerp(baseColor, chromatic, intensity);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample texture base (sem aberração para referência)
                float3 baseTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;
                
                // Sample texture com aberração cromática
                float3 chromaticColor = ChromaticAberration(IN.uv, _ChromaticAmount, _ChromaticIntensity);
                
                // Calcula gradiente baseado na distância real entre os pontos
                float3 lineDirection = _EndPoint.xyz - _StartPoint.xyz;
                float3 pointToStart = IN.worldPos - _StartPoint.xyz;
                
                // Projeta a posição atual na linha para obter posição normalizada (0-1)
                float projectedDistance = dot(pointToStart, normalize(lineDirection));
                float normalizedPosition = saturate(projectedDistance / _LineLength);
                
                // Calcula distância do centro da linha (0.5 = centro, 0 e 1 = extremidades)
                float centerDistance = abs(normalizedPosition - 0.5) * 2.0;
                float gradientMask = pow(1.0 - centerDistance, _GradientPower);
                
                // Intensifica as cores das extremidades
                half3 intensifiedEdgeColor = _EdgeColor.rgb * _EdgeIntensity;
                
                // Interpola entre cor das bordas intensificada e cor do centro
                half3 gradientColor = lerp(intensifiedEdgeColor, _CenterColor.rgb, gradientMask);
                
                // Garante que o gradiente não desapareça completamente nas bordas
                float minIntensity = max(0.1, 1.0 - _EdgeIntensity * 0.5); // Intensidade mínima baseada na edge intensity
                gradientColor = max(gradientColor, intensifiedEdgeColor * minIntensity);
                
                // Aplica gradiente apenas na intensidade, preservando a aberração cromática
                half3 finalColor = chromaticColor * saturate(gradientColor);
                
                return half4(finalColor,finalColor.x+finalColor.y);
            }
            ENDHLSL
        }
    }
}
