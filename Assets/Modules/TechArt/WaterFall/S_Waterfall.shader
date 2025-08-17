Shader "Unlit/Waterfall"
{
    Properties
    {
        _Specular("Specular", Vector) = (0,0,0,0)
        _Noise ("_Noise", 2D) = "white" {}
        _Color("Base Color", Color) = (0,0,1,1)
        _Color01("Foam Color", Color) = (1,1,1,1)
        _Color02("Specular Color", Color) = (1,1,1,1)
        _Color03("Dark Specular Color", Color) = (0,0.3,0.5,1)
        _Color04("XSpecular Color", Color) = (0.8,0.9,1,1)
        _LerpFactors("Lerp Factors (Base, Foam, Spec, Dark, X)", Vector) = (0.5, 0.7, 0.8, 0.3)
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

            sampler2D _Noise;
            float4 _Specular;
            float3 _Color;
            float3 _Color01;
            float3 _Color02;
            float3 _Color03;
            float3 _Color04;
            float4 _LerpFactors;

            float specular(float2 uv, float velocity, float xscale)
            {
                float2 st = float2(uv.x , uv.y);
                float n = tex2D(_Noise, st * float2(xscale, _Specular.z) - float2(0, _Time.y * velocity));
                n = smoothstep( _Specular.x, _Specular.y, n);
                n -= smoothstep( _Specular.x + .1, _Specular.y, n);
                n = step(.1, n);
                return clamp(n, 0, 1);
            }
            
            float BaseSpecular(float2 uv, float xscale, float yscale, float velocity)
            {
                float2 st = float2(uv.x * xscale, uv.y * yscale);
                float n = tex2D(_Noise, st + float2(0, _Time.y * velocity)) * 2;
                n = saturate(n);
                n = smoothstep(n , st.y  , 1.);
                return n;
            }
            
            float FoamBaseSpecular(float2 uv, float xscale, float yscale, float velocity)
            {
                float2 st = float2(uv.x * xscale, uv.y * yscale);
                float n = tex2D(_Noise, st * float2(xscale, yscale) + float2(0, _Time.y * velocity)) * 2;
                float y = saturate(1. - abs(uv.y - -0.88)) * 1;
                n = smoothstep(n , y, 1.);
                return n;
            }
            
            float darkBaseOccSpecular(float2 uv, float xscale, float yscale, float velocity)
            {
                float2 st = float2(uv.x * xscale , uv.y * yscale * .5 + .1 );
                float n = tex2D(_Noise, st + float2(_Time.y * .01, _Time.y * velocity )) * 2;
                n = smoothstep(n, st.y * 10, 1.);
                float n0 = smoothstep(n, st.y * 01, 1.);
                n = smoothstep(n, n0, 0);
                return n;
            }
            
            float xSpecular(float2 uv, float xscale, float yscale, float velocity)
            {
                float t = tex2D(_Noise, uv * float2(xscale * 5 , yscale) + float2(0, _Time.y * .1));
                float y = 1. - saturate(abs(uv.y - _Specular.w) * t * 100) * 2;
                y = step(.1, y);
                return y;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate all specular effects
                float spec = specular(i.uv, .03, 2);
                spec += saturate(specular(i.uv, .045, 1.3) * .7);
                spec = saturate(spec);

                float n = saturate(tex2D(_Noise, i.uv * float2(2,0)) * 5);
                float darkSpec = darkBaseOccSpecular(i.uv, 2, n * 4, .3);
                float darkSpecTop = darkBaseOccSpecular(i.uv, 1, n * 8, .35);
                
                // Apply colors to each effect with lerp
                float3 baseCol = _Color;
                baseCol = lerp(baseCol, _Color02, darkSpec);
                baseCol = lerp(baseCol, _Color03, darkSpecTop);
                baseCol = lerp(baseCol, _Color01, spec);
                return float4(baseCol, 1);
            }
            ENDCG
        }
    }
}