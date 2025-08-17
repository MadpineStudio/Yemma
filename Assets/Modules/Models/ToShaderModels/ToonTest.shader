Shader "Unlit/FaceShadow"
{
    Properties
    {
        _LightSmooth("_LightSmooth", Range(0,1)) = 0
        _ShadowColor("_ShadowColor", Color) = (0,0,0,0)
        _LightColor("_LightSmooth", Color) = (1,1,1,1)
        
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowTex ("Shadow Texture", 2D) = "white" {}
        _LightDir ("Light Direction", Vector) = (0, 0, 1, 0)
        _Controller ("_Controller", Range(0,.1)) =0 
        _HeadForward("_HeadForward", Vector) = (0,0,0,0)
        _HeadRight("_HeadRight", Vector) = (0,0,0,0)
        _MainLightDir("_MainLightDir", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define PI 3.14159265
            
            struct v2f
            {
                float2 uv_Main : TEXCOORD0;
                float2 uv_Shadow : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 normals: NORMAL;
            };

            float _LightSmooth;
            float3 _ShadowColor;
            float3 _LightColor;
            
            float3 _HeadForward;
            float3 _HeadRight;
            float3 _MainLightDir;
            
            sampler2D _MainTex;
            sampler2D _ShadowTex;
            float4 _MainTex_ST;
            float4 _ShadowTex_ST;
            float3 _LightDir;
            float _Controller;
            
            v2f vert(float4 vertex : POSITION, float4 normals: NORMAL, float2 uv : TEXCOORD0)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                o.uv_Main = TRANSFORM_TEX(uv, _MainTex);
                o.uv_Shadow = TRANSFORM_TEX(uv, _ShadowTex);
                o.normals = mul(unity_ObjectToWorld, float4(normals.xyz, 0.));
                return o;
            }
        float ComputeShadow(float dotF, float dotR, sampler2D _ShadowTex, float2 uv)
        {
            // Passo 1: Verifica se a luz está na frente do objeto (dotF > 0)
            float dotFStep = step(0, dotF);

            // Passo 2: Calcula o ângulo entre a luz e o vetor direito (headRight)
            float dotRAcos = acos(clamp(dotR, -1.0, 1.0)) / PI; // Normaliza para [0, 1]
            float dotRAcosDir = (dotR < 0.0) ? (1.0 - dotRAcos) : dotRAcos;

            // Passo 3: Amostra a textura de sombra
            float4 shadowTex = tex2D(_ShadowTex, uv);
            float texShadowDir = (dotR < 0.0) ? shadowTex.g : shadowTex.r;

            // Passo 4: Aplica transição suave
            float shadowTransition = smoothstep(dotRAcosDir - _Controller, dotRAcosDir + _Controller, texShadowDir);
            // float shadowTransition = step(dotRAcosDir, texShadowDir);

            // Passo 5: Combina os resultados
            return shadowTransition * dotFStep;
        }
        float3 ComputeColor(float shadows)
        {
            return lerp(_ShadowColor, _LightColor, shadows);
        }
        float3 CalculateLights(v2f i)
        {
            float l = dot(-_MainLightDir, i.normals.xyz);
            l = smoothstep(0, _LightSmooth, l);
            float3 color = lerp(_ShadowColor, _LightColor, l);
            return color;
        }
        // Converter blender para unity - ToonShadows: pass 01
        float ToonShadows()
        {
            
        }
        float4 frag(v2f i) : SV_Target
        {
            // Passo 1: Normaliza os vetores
            float2 headForward = normalize(float2(_HeadForward.x, _HeadForward.z));
            float2 headRight = normalize(float2(_HeadRight.x, _HeadRight.z));
            float2 lightDir = normalize(float2(_MainLightDir.x, _MainLightDir.z));
            
            // Passo 2: Calcula os produtos escalares
            float dotF = dot(headForward, lightDir); // Produto escalar entre frente e luz
            float dotR = dot(headRight, lightDir);   // Produto escalar entre direita e luz
            
            float shadowDir = ComputeShadow(dotF, dotR, _ShadowTex, i.uv_Shadow);
            float3 color = ComputeColor(shadowDir);
            
            float3 shadows = CalculateLights(i);
            return float4(color, 1);
            return float4(shadowDir.xxx, 1);
        }

        ENDCG
        }
    }
}
