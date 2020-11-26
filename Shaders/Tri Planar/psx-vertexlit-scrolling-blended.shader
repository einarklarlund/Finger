// mostly from https://github.com/dsoft20/psx_retroshader
// mixed with parts of https://github.com/keijiro/Retro3D
// under MIT license by Robert Yang (https://debacle.us)

Shader "psx/vertexlit-scrolling-blended" {
	Properties{
		_Color("Color", Color) = (0.5, 0.5, 0.5, 1)
		_GeoRes("Geometric Resolution", Float) = 40

        _DiffuseMapY ("Diffuse Map Y", 2D)  = "white" {}
        _DiffuseMapX ("Diffuse Map X", 2D)  = "white" {}
        _DiffuseMapZ ("Diffuse Map Z", 2D)  = "white" {}
        _TextureScale ("Texture Scale",float) = 1
        _TriplanarBlendSharpness ("Blend Sharpness",float) = 1
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
				float4 worldPos : TEXCOORD0;
				half3 normal : TEXCOORD1;
                int val : TEXCOORD2;
                float distance : TEXCOORD3;
			};

			float4 _MainTex_ST;
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			float4 _Color;
			float _GeoRes;
				   
			sampler2D _DiffuseMapY;
			sampler2D _DiffuseMapX;
			sampler2D _DiffuseMapZ;
			float _TextureScale;
			float _TriplanarBlendSharpness;

			v2f vert(appdata_full v)
			{
				v2f o;

				// including the line below will tell Unity not to upgrade the matrix mul() operations below...
				// UNITY_SHADER_NO_UPGRADE
				float4 wp = mul(UNITY_MATRIX_MV, v.vertex);
				float3 product = wp.xyz * _GeoRes;
				wp.xyz = floor(product) / _GeoRes;

				float4 sp = mul(UNITY_MATRIX_P, wp);
				o.pos = sp;

				//Vertex lighting 
				o.color = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 1.0);
				o.color *= v.color; // vertex color support

				float distance = length(UnityObjectToClipPos(v.vertex));

				//Affine Texture Mapping
				float4 affinePos = wp;				
				// o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				// o.uv_MainTex *= distance + (wp.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2;
				o.worldPos = wp;
				o.distance = distance;
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
				// Find our UVs for each axis based on world position of the fragment.
				half2 yUV = IN.worldPos.xz * (IN.distance + (IN.worldPos.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / IN.distance / 2) / _TextureScale + _Time;
				half2 xUV = IN.worldPos.zy * (IN.distance + (IN.worldPos.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / IN.distance / 2) / _TextureScale + _Time;
				half2 zUV = IN.worldPos.xy * (IN.distance + (IN.worldPos.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / IN.distance / 2) / _TextureScale + _Time;
				// Now do texture samples from our diffuse map with each of the 3 UV set's we've just made.
				half3 yDiff = tex2D (_DiffuseMapY, yUV);
				half3 xDiff = tex2D (_DiffuseMapX, xUV);
				half3 zDiff = tex2D (_DiffuseMapZ, zUV);

				// Get the absolute value of the world normal.
				// Put the blend weights to the power of BlendSharpness, the higher the value, 
				// the sharper the transition between the planar maps will be.
				half3 blendWeights = pow (abs(IN.normal), _TriplanarBlendSharpness);
				// Divide our blend mask by the sum of it's components, this will make x+y+z=1
				blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
				// Finally, blend together all three samples based on the blend mask.
				half3 c = xDiff * blendWeights.x + yDiff * blendWeights.y + zDiff * blendWeights.z;

				half4 color = half4(c.r, c.g, c.b, 0) * (IN.colorFog.a);
				color.rgb += IN.colorFog.rgb * (1 - IN.colorFog.a);

                clip(-IN.val);

				return color;
			}
			
			ENDCG
		}
	}
}