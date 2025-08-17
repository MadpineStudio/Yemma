Shader "Unlit/Billboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Textura opcional para o billboard
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex; // Textura opcional
            float4 _MainTex_ST; // Scale e Offset da textura

            // Função para calcular a posição do billboard
            float4 Billboard(float4 vertex)
            {
                // Remove a rotação do objeto para que ele sempre fique de frente para a câmera
                float3 vpos = mul((float3x3)unity_ObjectToWorld, vertex.xyz);
                float4 worldPos = float4(unity_ObjectToWorld._m03_m13_m23, 1.0);

                // Vetor para a câmera
                float3 forward = -normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                float3 right = normalize(cross(float3(0, 1, 0), forward)); // Eixo Y como "up"
                float3 up = normalize(cross(forward, right));

                vpos = worldPos.xyz + right * vpos.x + up * vpos.y;
                return mul(UNITY_MATRIX_VP, float4(vpos, 1.0));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = Billboard(v.vertex); // Chama a função de billboard
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // Aplica scale e offset da textura
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Amostra a textura (opcional)
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}