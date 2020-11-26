Shader "My Shaders/Test Surface Shader"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Hatch0 ("Hatch 0 (lightest)", 2D) = "white" {}
        _Hatch1 ("Hatch 1", 2D) = "white" {}
        _Hatch2 ("Hatch 2", 2D) = "white" {}
        _Hatch3 ("Hatch 3", 2D) = "white" {}
        _Hatch4 ("Hatch 4", 2D) = "white" {}
        _Hatch5 ("Hatch 5", 2D) = "white" {}
        _Repeat ("Repeat Tile", float) = 4
    }
   
    SubShader {
        Tags { "RenderType" = "Opaque" }
        Blend DstColor Zero
        CGPROGRAM
        #pragma surface surf CrossHatch

        sampler2D _MainTex;
        sampler2D _Hatch0;
        sampler2D _Hatch1;
        sampler2D _Hatch2;
        sampler2D _Hatch3;
        sampler2D _Hatch4;
        sampler2D _Hatch5;
        fixed _Repeat;
       
        struct MySurfaceOutput
        {
            fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            fixed Gloss;
            fixed Alpha;
            fixed val;
            float2 uv;
        };
       
        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
        };
     
        void surf (Input IN, inout MySurfaceOutput o) {
//uncomment to use object space hatching
           o.uv = IN.uv_MainTex * _Repeat;
// uncomment to use screen space hatching
            // o.uv = IN.screenPos.xy * _Repeat / IN.screenPos.w;
            // half v = length(tex2D (_MainTex, IN.uv_MainTex).rgb) * 0.33;
            half v = dot(tex2D(_MainTex, IN.uv_MainTex), half3(0.3, 0.59, 0.11));
            o.val = v;
        }
       
        half4 LightingCrossHatch (MySurfaceOutput s, half3 lightDir, half atten)
        {
            half NdotL = dot (s.Normal, lightDir);

            half4 c0 = tex2D(_Hatch0, s.uv);
            half4 c1 = tex2D(_Hatch1, s.uv);
            half4 c2 = tex2D(_Hatch2, s.uv);
            half4 c3 = tex2D(_Hatch3, s.uv);
            half4 c4 = tex2D(_Hatch4, s.uv);
            half4 c5 = tex2D(_Hatch5, s.uv);
            half4 c;
           
            half v = saturate(length(_LightColor0.rgb) * (NdotL * atten * 2) * s.val);
           
            c.rgb = lerp(c5, c4, v);
            c.rgb = lerp(c.rgb, c3, v);
            c.rgb = lerp(c.rgb, c2, v);
            c.rgb = lerp(c.rgb, c1, v);
            c.rgb = lerp(c.rgb, c0, v);
            c.rgb = lerp(c.rgb, half3(1, 1, 1), v);
            c.a = s.Alpha;
            return c;
        }
       
      ENDCG
    }
    Fallback "Diffuse"
}