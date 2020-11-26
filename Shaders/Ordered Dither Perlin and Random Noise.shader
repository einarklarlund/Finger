Shader "My Shaders/Ordered Dither Perlin and Random Noise"{
	//show values to edit in inspector
    Properties{
        [Header(Base parameters)]
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Specular ("Specular Color", Color) = (1,1,1,1)
        [HDR] _Emission ("Emission", color) = (0 ,0 ,0 , 1)

        [Header(Dither parameters)]
        _DitherPattern ("Dithering Pattern", 2D) = "white" {}
        _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
        _Color2 ("Dither Color 2", Color) = (1, 1, 1, 1)
		_Blend ("Blending of color 2 and texture color", Range(0, 1)) = 0

		[Header(Random Noise parameters)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}
		_RandomNoiseCutoff ("Cutoff value for random noise", Range(0, 1.5)) = 0

		[Header(Perlin Noise parameters)]
		_CellSize ("Cell Size", Range(0, 100)) = 1
		_CellSizeOverride ("Cell Size Override", float) = 0
		_ScrollSpeed ("Scroll Speed", Range(0, 1)) = 1
		_PerlinNoiseCutoff ("Cutoff value for perlin noise", Range(0, 1.5)) = 0

        [Header(Lighting parameters)]
        _ShadowTint ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
        [IntRange]_StepAmount ("Shadow Steps", Range(1, 16)) = 2
        _StepWidth ("Step Size", Range(0, 1)) = 0.25
        _SpecularSize ("Specular Size", Range(0, 1)) = 0.1
        _SpecularFalloff ("Specular Falloff", Range(0, 2)) = 1
    }

    SubShader {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        CGPROGRAM

        //the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
        //our surface shader function is called surf and we use the default PBR lighting model
        #pragma surface surf Stepped fullforwardshadows
        #pragma target 3.0

        //texture and transforms of the texture, color and lighting
        sampler2D _MainTex;
        fixed4 _Tint;
        half3 _Emission;
        fixed4 _Specular;

        //The dithering pattern
        sampler2D _DitherPattern;
        float4 _DitherPattern_TexelSize;

		//Random noise vars
		uniform sampler2D _NoiseTex;
        float _RandomNoiseCutoff;

		//Perlin noise vars
		float _CellSize;
		float _CellSizeOverride;
		float _ScrollSpeed;
        float _PerlinNoiseCutoff;

        //Dither colors
        float4 _Color1;
        float4 _Color2;
        float _Blend;

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
            float4 screenPos;	
		    float3 worldPos;
        };
        
        //toon lighting properties
        float3 _ShadowTint;
        float _StepWidth;
        float _StepAmount;
        float _SpecularSize;
        float _SpecularFalloff;

        struct ToonSurfaceOutput{
            fixed3 Albedo;
            half3 Emission;
            fixed3 Specular;
            fixed Alpha;
            fixed3 Normal;
        };

        float rand(in float2 uv) {
            float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
            return abs(noise.x + noise.y) + 0.1;
        }

		float4 getNoise(float2 uv) 
		{
			float4 noise = tex2D(_NoiseTex, 10000 + uv + _Time * 50);
			// noise = mad(noise, 2.0, -0.5);
			return noise;
		}

		float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719)){
			//make value smaller to avoid artefacts
			float3 smallValue = sin(value);
			//get scalar value from 3d vector
			float random = dot(smallValue, dotDir);
			//make value more random by making it bigger and then taking teh factional part
			random = frac(sin(random) * 143758.5453);
			return random;
		}

		float3 rand3dTo3d(float3 value){
			return float3(
				rand3dTo1d(value, float3(12.989, 78.233, 37.719)),
				rand3dTo1d(value, float3(39.346, 11.135, 83.155)),
				rand3dTo1d(value, float3(73.156, 52.235, 09.151))
			);
		}

		inline float easeIn(float interpolator){
			return interpolator * interpolator;
		}

		float easeOut(float interpolator){
			return 1 - easeIn(1 - interpolator);
		}

		float easeInOut(float interpolator){
			float easeInValue = easeIn(interpolator);
			float easeOutValue = easeOut(interpolator);
			return lerp(easeInValue, easeOutValue, interpolator);
		}

		float perlinNoise(float3 value){
			float3 fraction = frac(value);

			float interpolatorX = easeInOut(fraction.x);
			float interpolatorY = easeInOut(fraction.y);
			float interpolatorZ = easeInOut(fraction.z);

			float3 cellNoiseZ[2];
			[unroll]
			for(int z=0;z<=1;z++){
				float3 cellNoiseY[2];
				[unroll]
				for(int y=0;y<=1;y++){
					float3 cellNoiseX[2];
					[unroll]
					for(int x=0;x<=1;x++){
						float3 cell = floor(value) + float3(x, y, z);
						float3 cellDirection = rand3dTo3d(cell) * 2 - 1;
						float3 compareVector = fraction - float3(x, y, z);
						cellNoiseX[x] = dot(cellDirection, compareVector);
					}
					cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
				}
				cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
			}
			float3 noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);
			return noise;
		}

        //our lighting function. Will be called once per light
        float4 LightingStepped(ToonSurfaceOutput s, float3 lightDir, half3 viewDir, float shadowAttenuation){
            //how much does the normal point towards the light?
            float towardsLight = dot(s.Normal, lightDir);

            //stretch values so each whole value is one step
            towardsLight = towardsLight / _StepWidth;
            //make steps harder
            float lightIntensity = floor(towardsLight);

            // calculate smoothing in first pixels of the steps and add smoothing to step, raising it by one step
            // (that's fine because we used floor previously and we want everything to be the value above the floor value, 
            // for example 0 to 1 should be 1, 1 to 2 should be 2 etc...)
            float change = fwidth(towardsLight);
            float smoothing = smoothstep(0, change, frac(towardsLight));
            lightIntensity = lightIntensity + smoothing;

            // bring the light intensity back into a range where we can use it for color
            // and clamp it so it doesn't do weird stuff below 0 / above one
            lightIntensity = lightIntensity / _StepAmount;
            lightIntensity = saturate(lightIntensity);

        #ifdef USING_DIRECTIONAL_LIGHT
            //for directional lights, get a hard vut in the middle of the shadow attenuation
            float attenuationChange = fwidth(shadowAttenuation) * 0.5;
            float shadow = smoothstep(0.5 - attenuationChange, 0.5 + attenuationChange, shadowAttenuation);
        #else
            //for other light types (point, spot), put the cutoff near black, so the falloff doesn't affect the range
            float attenuationChange = fwidth(shadowAttenuation);
            float shadow = smoothstep(0, attenuationChange, shadowAttenuation);
        #endif
            lightIntensity = lightIntensity * shadow;

            //calculate how much the surface points points towards the reflection direction
            float3 reflectionDirection = reflect(lightDir, s.Normal);
            float towardsReflection = dot(viewDir, -reflectionDirection);

            //make specular highlight all off towards outside of model
            float specularFalloff = dot(viewDir, s.Normal);
            specularFalloff = pow(specularFalloff, _SpecularFalloff);
            towardsReflection = towardsReflection * specularFalloff;

            //make specular intensity with a hard corner
            float specularChange = fwidth(towardsReflection);
            float specularIntensity = smoothstep(1 - _SpecularSize, 1 - _SpecularSize + specularChange, towardsReflection);
            //factor inshadows
            specularIntensity = specularIntensity * shadow;

            float4 color;
            //calculate final color
            color.rgb = s.Albedo * lightIntensity * _LightColor0.rgb;
            color.rgb = lerp(color.rgb, s.Specular * _LightColor0.rgb, saturate(specularIntensity));

            color.a = s.Alpha;
            return color;
        }

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout ToonSurfaceOutput o) {
            //read texture and write it to diffuse color
            float4 texColor = tex2D(_MainTex, i.uv_MainTex);
            texColor *= _Tint;

            // _DitherPattern_TexelSize.x = 0.25;
            // _DitherPattern_TexelSize.y = 0.25;

            //value from the dither pattern
            float2 screenPos = i.screenPos.xy / i.screenPos.w;
            float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
            float ditherValue = tex2D(_DitherPattern, ditherCoordinate).r;
			
			//apply noise to dither pattern value
            float noise = getNoise(i.uv_MainTex) * 1.5;
            if(noise + ditherValue <= _RandomNoiseCutoff) {
			    noise = mad(noise, 2.0, -0.5);
                ditherValue += noise;
            }

            float3 pValue;

			//calculate perlin noise values
            if(_CellSizeOverride == 0) {
    			pValue = i.worldPos / _CellSize;
            }
            else {
                pValue = i.worldPos / _CellSizeOverride;
            }
			pValue.x += _Time.x * _ScrollSpeed * 20;
			pValue.y += _Time.y * _ScrollSpeed;
			pValue.z += _Time.z * _ScrollSpeed / 2;
 
			//get noise and adjust it to be ~0-1 range
			float pNoise = perlinNoise(pValue) + 0.5;
			pNoise = frac(pNoise * 6);

			// float pixelNoiseChange = fwidth(pNoise);
			// float heightLine = smoothstep(1-pixelNoiseChange, 1, pNoise);
			// heightLine += smoothstep(pixelNoiseChange, 0, pNoise);

			//apply perlin noise to value from dither
            if(ditherValue + pNoise <= _PerlinNoiseCutoff) {
                ditherValue += pNoise;
            }

            float ditheredValue = step(ditherValue, texColor);

            float4 blendColor = lerp(texColor, _Color2, _Blend);
            float4 col = lerp(blendColor, _Color1, ditheredValue);

            o.Albedo = col;
            float3 shadowColor = col.rgb * _ShadowTint;
            o.Emission = _Emission + shadowColor;
            o.Specular = _Specular;
        }
        ENDCG
    }
    FallBack "Standard"
}