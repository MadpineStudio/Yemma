Shader "Custom/PlaneTiltNoise"
{
    Properties
    {
        _TiltDirection ("Tilt Direction", Vector) = (1, 0, 0, 0) // Direção do tilt (xy)
        _Angle("_Angle", Float) = 0
        
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {} // Textura de noise
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "TiltUtils.hlsl"

            // Estruturas
            struct appdata
            {
                float4 vertex : POSITION; // Vértice no espaço local
                float2 uv : TEXCOORD0;   // Coordenadas UV
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // Posição no espaço de clip
                float2 uv : TEXCOORD0;   // Coordenadas UV
            };

            // Texturas
            sampler2D _MainTex;
            sampler2D _NoiseTex;

            // Parâmetros do tilt
            float _Angle;
            float2 _TiltDirection; // Direção do tilt (xy)

           
            v2f vert(appdata v)
            {
                v2f o;
                float angle = -_Angle;
                float2 tilt = _TiltDirection * angle;
                float3x3 rotationMatrixX = RotationMatrixX(tilt.y); // Inclinação no eixo X
                float3x3 rotationMatrixZ = RotationMatrixZ(tilt.x); // Inclinação no eixo Z
                float3x3 rotationMatrix = mul(rotationMatrixX, rotationMatrixZ);
                
                float3 rotatedVertex = mul(rotationMatrix, v.vertex.xyz);
                o.pos = UnityObjectToClipPos(float4(rotatedVertex, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 1;
            }
            ENDCG
        }
    }
}