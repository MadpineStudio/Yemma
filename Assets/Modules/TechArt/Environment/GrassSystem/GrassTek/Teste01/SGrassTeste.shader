Shader "Custom/Grass" {
    Properties {
        _InputMask ("Texture", 2D) = "white" {}
        _WindSpeed ("Wind Speed", Float) = 1.0
        _WindStrength ("Wind Strength", Float) = 0.1
        _BendStrength ("Bend Strength", Float) = 0.5
    }

    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _InputMask;  // Textura gerada pelo Compute Shader
            float _WindSpeed;
            float _WindStrength;
            float _BendStrength;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.uv = v.uv;

                // Lê a interação do jogador (direção + intensidade)
                float4 interaction = tex2Dlod(_InputMask, float4(v.uv, 0, 0));
                float2 bendDir = interaction.xy;
                float bendAmount = interaction.z;

                // Adiciona movimento do vento (opcional)
                float windWave = sin(_Time.y * _WindSpeed + v.vertex.x) * _WindStrength;

                // Aplica o displacement:
                // - Inclinação baseada na interação do jogador
                // - Vento para movimento natural
                v.vertex.xz += bendDir * bendAmount * _BendStrength;
                v.vertex.x += windWave;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return tex2D(_InputMask, 1.-i.uv);
            }
            ENDCG
        }
    }
}