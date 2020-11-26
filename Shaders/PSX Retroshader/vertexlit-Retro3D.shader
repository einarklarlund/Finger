﻿// mostly from https://github.com/dsoft20/psx_retroshader
// mixed with parts of https://github.com/keijiro/Retro3D
// under MIT license by Robert Yang (https://debacle.us)

Shader "psx/VertexLit-Retro3D" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (0.5, 0.5, 0.5, 1)
		_GeoRes("Geometric Resolution", Float) = 40
	}
		SubShader {
			Tags { "RenderType" = "Opaque" }
			LOD 200

			Pass {
			Lighting On
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				half4 color : COLOR0;
				half4 colorFog : COLOR1;
				float2 uv_MainTex : TEXCOORD0;
				half3 normal : TEXCOORD1;
                int val : TEXCOORD2;
			};

			float4 _MainTex_ST;
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			float4 _Color;
			float _GeoRes;

			float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719))
			{
				//make value smaller to avoid artefacts
				float3 smallValue = sin(value);
				//get scalar value from 3d vector
				float random = dot(smallValue, dotDir);
				//make value more random by making it bigger and then taking teh factional part
				random = frac(sin(random) * 143758.5453);
				return random - 0.5;
			}

			v2f vert(appdata_full v)
			{
				v2f o;

				// including the line below will tell Unity not to upgrade the matrix mul() operations below...
				// UNITY_SHADER_NO_UPGRADE
				float4 wp = mul(UNITY_MATRIX_MV, v.vertex);
				// wp.xyz = floor(wp.xyz * _GeoRes) / _GeoRes;
				float3 displacement = wp.xyz - floor(wp.xyz * _GeoRes) / _GeoRes ;
				displacement *= sign(rand3dTo1d(wp.xyz));
				displacement /= 3;
				wp.xyz += displacement;

				float4 sp = mul(UNITY_MATRIX_P, wp);
				o.pos = sp;

				//Vertex lighting 
				o.color = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 1.0);
				o.color *= v.color; // vertex color support

				float distance = length(UnityObjectToClipPos(v.vertex));

				//Affine Texture Mapping
				float4 affinePos = wp;				
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_MainTex *= distance + (wp.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2;
				o.normal = distance + (wp.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2;

				//Fog
				float4 fogColor = unity_FogColor;
				float fogDensity = (unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart);
				o.normal.g = fogDensity;
				o.normal.b = 1;

				o.colorFog = fogColor;
				o.colorFog.a = clamp(fogDensity,0,1);

                // Cut out polygons
                o.val = 0;
                if (distance > unity_FogStart.z + unity_FogColor.a * 255)
                {
                    o.val = 1;
                }

				return o;
			}

			sampler2D _MainTex;

			float4 frag(v2f IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv_MainTex / IN.normal.r) * IN.color * _Color;
				half4 color = c*(IN.colorFog.a);
				color.rgb += IN.colorFog.rgb*(1 - IN.colorFog.a);
				color.a = c.a;

                clip(-IN.val);

				return color;
			}
			
			ENDCG
		}
	}
}