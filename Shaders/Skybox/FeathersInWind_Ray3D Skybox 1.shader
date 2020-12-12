// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/FeathersInWind_Ray3D Skybox" {
Properties {
    [Header(Speed Scale)]
    _Speed("Speed",Range(0.0,5.0)) = 1.0
    _Scale("Scale",Range(1.0,10.0)) = 1.0

    [Header(BG Colors)]
    _BGColorA("Color A",Color) = (0.2, 0.2, 0.7, 1.0)
    _BGColorB("Color B",Color) = (1.0, 0.6, 0.1, 1.0)

    [Header(Feather Properties)]
    _Strandcount("Strand count", float) = 50.0
    _waveLength("wave Length", float) = 0.2
    _XCutRange("XCutRange", float) = 0.9
    
    [Header(3D Settings)]
    _MaxIter("Max Iteration", float) = 10
    _FeatherXRange("Feather X Range", Range(0.0,10.0)) = 8.0
    _FeatherYRange("Feather Y Range", Range(0.0,5.0)) = 3.0    
    _Rotation ("Rotation", Range(0, 360)) = 0

    [Header(Skybox)]
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _MainTex ("Spherical  (HDR)", 2D) = "grey" {}
    [KeywordEnum(6 Frames Layout, Latitude Longitude Layout)] _Mapping("Mapping", Float) = 1
    [Enum(360 Degrees, 0, 180 Degrees, 1)] _ImageType("Image Type", Float) = 0
    [Toggle] _MirrorOnBack("Mirror on Back", Float) = 0
    [Enum(None, 0, Side by Side, 1, Over Under, 2)] _Layout("3D Layout", Float) = 0
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        #pragma multi_compile_local __ _MAPPING_6_FRAMES_LAYOUT

        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        half4 _MainTex_HDR;
        half4 _Tint;
        half _Exposure;
        float _Rotation;
        float4 _MainTex_ST;

        fixed _Speed;
        fixed _Scale;
        fixed _Strandcount;
        fixed _waveLength;
        fixed _XCutRange;
        fixed _MaxIter;
        fixed _FeatherXRange;
        fixed _FeatherYRange;

        fixed4 _BGColorA;
        fixed4 _BGColorB;

#ifndef _MAPPING_6_FRAMES_LAYOUT
        bool _MirrorOnBack;
        int _ImageType;
        int _Layout;
#endif

#ifndef _MAPPING_6_FRAMES_LAYOUT
        inline float2 ToRadialCoords(float3 coords)
        {
            float3 normalizedCoords = normalize(coords);
            float latitude = acos(normalizedCoords.y);
            float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
            float2 sphereCoords = float2(longitude, latitude) * float2(0.5/UNITY_PI, 1.0/UNITY_PI);
            return float2(0.5,1.0) - sphereCoords;
        }
#endif

#ifdef _MAPPING_6_FRAMES_LAYOUT
        inline float2 ToCubeCoords(float3 coords, float3 layout, float4 edgeSize, float4 faceXCoordLayouts, float4 faceYCoordLayouts, float4 faceZCoordLayouts)
        {
            // Determine the primary axis of the normal
            float3 absn = abs(coords);
            float3 absdir = absn > float3(max(absn.y,absn.z), max(absn.x,absn.z), max(absn.x,absn.y)) ? 1 : 0;
            // Convert the normal to a local face texture coord [-1,+1], note that tcAndLen.z==dot(coords,absdir)
            // and thus its sign tells us whether the normal is pointing positive or negative
            float3 tcAndLen = mul(absdir, float3x3(coords.zyx, coords.xzy, float3(-coords.xy,coords.z)));
            tcAndLen.xy /= tcAndLen.z;
            // Flip-flop faces for proper orientation and normalize to [-0.5,+0.5]
            bool2 positiveAndVCross = float2(tcAndLen.z, layout.x) > 0;
            tcAndLen.xy *= (positiveAndVCross[0] ? absdir.yx : (positiveAndVCross[1] ? float2(absdir[2],0) : float2(0,absdir[2]))) - 0.5;
            // Clamp values which are close to the face edges to avoid bleeding/seams (ie. enforce clamp texture wrap mode)
            tcAndLen.xy = clamp(tcAndLen.xy, edgeSize.xy, edgeSize.zw);
            // Scale and offset texture coord to match the proper square in the texture based on layout.
            float4 coordLayout = mul(float4(absdir,0), float4x4(faceXCoordLayouts, faceYCoordLayouts, faceZCoordLayouts, faceZCoordLayouts));
            tcAndLen.xy = (tcAndLen.xy + (positiveAndVCross[0] ? coordLayout.xy : coordLayout.zw)) * layout.yz;
            return tcAndLen.xy;
        }
#endif

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        float2x2 Rotate(float angle)
        {
            Float s = sin(angle), c = cos(angle);
            return float2x2(c, -s, s, c);
        }

        fixed Feather(fixed2 p)
        {
            fixed d = length(p - fixed2(0, clamp(p.y, -0.3, 0.3))); 
            fixed r = 1;
            fixed m = smoothstep(0.01, 0.0, d-r);

            fixed x = _XCutRange*abs(p.x) / r;
            fixed wave = (1.0 - x) * sqrt(x) + x*(1.0 - sqrt(1.0 - x));
            fixed y = (p.y - wave *_waveLength) * _Strandcount;
            fixed id = floor(y);
            fixed n = frac(sin(id*564.32) * 763.0);  // random number
            fixed shade = 0.5;
            fixed strandLength = 0.7;

            fixed strand = smoothstep(0.3,0.0, abs(frac(y) - 0.5) - 0.3);
            strand *= smoothstep(0.1, -0.2, x - strandLength);

            d = length(p - fixed2(0, clamp(p.y, -0.45, 0.1))); 

            return m * shade * strand;
            // return max( m * strand * shade, stem);
        }

        fixed3 Transform(fixed3 p, fixed angle)
        {
            p.xz = mul(p.xz, Rotate(angle));
            p.xy = mul(p.xy ,Rotate(angle *0.7));
            return p;
        }

        fixed4 FeatherBall(fixed3 ro, fixed3 rd, fixed3 pos, fixed angle)
        {
            fixed4 col = fixed4(0,0,0,0);

            fixed t = dot(pos - ro , rd);
            fixed3 p = ro + rd *t; // point of hit 
            fixed y = length(pos - p);  

            if(y < 1.0) // we have a hit
            {
                fixed x = sqrt(1.0 - y);

                fixed3 pF = ro + rd * (t-x) - pos; // front intersections
                pF = Transform(pF , angle);
                fixed2 uvF = fixed2(atan2(pF.x, pF.z), pF.y); // uv -pi<>+pi , -1<>+1
                uvF *= fixed2(0.3,0.5);
                fixed f = Feather(uvF);
                fixed4 front = fixed4(fixed3(f,f,f), f);

                fixed3 pB = ro + rd * (t+x) - pos; // back intersection
                pB = Transform(pB , angle);
                fixed2 uvB = fixed2(atan2(pB.x, pB.z), pB.y); 
                uvB *= fixed2(0.3,0.5);
                fixed b = Feather(uvB);
                fixed4 back = fixed4(fixed3(b,b,b), b);

                col = lerp(back, front , front.a);
            }

            return col;
        }


        struct appdata_t {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
#ifdef _MAPPING_6_FRAMES_LAYOUT
            float3 layout : TEXCOORD1;
            float4 edgeSize : TEXCOORD2;
            float4 faceXCoordLayouts : TEXCOORD3;
            float4 faceYCoordLayouts : TEXCOORD4;
            float4 faceZCoordLayouts : TEXCOORD5;
#else
            float2 image180ScaleAndCutoff : TEXCOORD1;
            float4 layout3DScaleAndOffset : TEXCOORD2;
#endif
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;
#ifdef _MAPPING_6_FRAMES_LAYOUT
            // layout and edgeSize are solely based on texture dimensions and can thus be precalculated in the vertex shader.
            float sourceAspect = float(_MainTex_TexelSize.z) / float(_MainTex_TexelSize.w);
            // Use the halfway point between the 1:6 and 3:4 aspect ratios of the strip and cross layouts to
            // guess at the correct format.
            bool3 aspectTest =
                sourceAspect >
                float3(1.0, 1.0f / 6.0f + (3.0f / 4.0f - 1.0f / 6.0f) / 2.0f, 6.0f / 1.0f + (4.0f / 3.0f - 6.0f / 1.0f) / 2.0f);
            // For a given face layout, the coordinates of the 6 cube faces are fixed: build a compact representation of the
            // coordinates of the center of each face where the first float4 represents the coordinates of the X axis faces,
            // the second the Y, and the third the Z. The first two float componenents (xy) of each float4 represent the face
            // coordinates on the positive axis side of the cube, and the second (zw) the negative.
            // layout.x is a boolean flagging the vertical cross layout (for special handling of flip-flops later)
            // layout.yz contains the inverse of the layout dimensions (ie. the scale factor required to convert from
            // normalized face coords to full texture coordinates)
            if (aspectTest[0]) // horizontal
            {
                if (aspectTest[2])
                { // horizontal strip
                    o.faceXCoordLayouts = float4(0.5,0.5,1.5,0.5);
                    o.faceYCoordLayouts = float4(2.5,0.5,3.5,0.5);
                    o.faceZCoordLayouts = float4(4.5,0.5,5.5,0.5);
                    o.layout = float3(-1,1.0/6.0,1.0/1.0);
                }
                else
                { // horizontal cross
                    o.faceXCoordLayouts = float4(2.5,1.5,0.5,1.5);
                    o.faceYCoordLayouts = float4(1.5,2.5,1.5,0.5);
                    o.faceZCoordLayouts = float4(1.5,1.5,3.5,1.5);
                    o.layout = float3(-1,1.0/4.0,1.0/3.0);
                }
            }
            else
            {
                if (aspectTest[1])
                { // vertical cross
                    o.faceXCoordLayouts = float4(2.5,2.5,0.5,2.5);
                    o.faceYCoordLayouts = float4(1.5,3.5,1.5,1.5);
                    o.faceZCoordLayouts = float4(1.5,2.5,1.5,0.5);
                    o.layout = float3(1,1.0/3.0,1.0/4.0);
                }
                else
                { // vertical strip
                    o.faceXCoordLayouts = float4(0.5,5.5,0.5,4.5);
                    o.faceYCoordLayouts = float4(0.5,3.5,0.5,2.5);
                    o.faceZCoordLayouts = float4(0.5,1.5,0.5,0.5);
                    o.layout = float3(-1,1.0/1.0,1.0/6.0);
                }
            }
            // edgeSize specifies the minimum (xy) and maximum (zw) normalized face texture coordinates that will be used for
            // sampling in the texture. Setting these to the effective size of a half pixel horizontally and vertically
            // effectively enforces clamp mode texture wrapping for each individual face.
            o.edgeSize.xy = _MainTex_TexelSize.xy * 0.5 / o.layout.yz - 0.5;
            o.edgeSize.zw = -o.edgeSize.xy;
#else // !_MAPPING_6_FRAMES_LAYOUT
            // Calculate constant horizontal scale and cutoff for 180 (vs 360) image type
            if (_ImageType == 0)  // 360 degree
                o.image180ScaleAndCutoff = float2(1.0, 1.0);
            else  // 180 degree
                o.image180ScaleAndCutoff = float2(2.0, _MirrorOnBack ? 1.0 : 0.5);
            // Calculate constant scale and offset for 3D layouts
            if (_Layout == 0) // No 3D layout
                o.layout3DScaleAndOffset = float4(0,0,1,1);
            else if (_Layout == 1) // Side-by-Side 3D layout
                o.layout3DScaleAndOffset = float4(unity_StereoEyeIndex,0,0.5,1);
            else // Over-Under 3D layout
                o.layout3DScaleAndOffset = float4(0, 1-unity_StereoEyeIndex,1,0.5);
#endif
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
#ifdef _MAPPING_6_FRAMES_LAYOUT
            float2 tc = ToCubeCoords(i.texcoord, i.layout, i.edgeSize, i.faceXCoordLayouts, i.faceYCoordLayouts, i.faceZCoordLayouts);
#else
            float2 tc = ToRadialCoords(i.texcoord);
            if (tc.x > i.image180ScaleAndCutoff[1])
                return half4(0,0,0,1);
            tc.x = fmod(tc.x*i.image180ScaleAndCutoff[0], 1);
            tc = (tc + i.layout3DScaleAndOffset.xy) * i.layout3DScaleAndOffset.zw;
#endif

            half4 tex = tex2D (_MainTex, tc);
            half3 c = DecodeHDR (tex, _MainTex_HDR);
            c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            c *= _Exposure;
        
            fixed2 uv = (tc - 0.5) *_Scale;

            // fixed4 col = lerp(_BGColorA, _BGColorB, uv.y+0.5);
            fixed4 col = lerp(fixed4(0.234, 0.1681875, 0.1681875, 1.0), fixed4(0.4435955, 0.4435955, 0.56, 1), uv.y * 0.5 - 0.4);

            fixed3 ro = fixed3(0,0,-3);
            fixed3 rd = normalize(fixed3(uv,1));

            // fixed speed = _Time.y * _Speed;
            fixed speed = _Time.y * 0.5;
            
            // for(fixed i = 0; i < 1.0; i+= 1.0/_MaxIter)
            for(fixed i = 0; i < 1.0; i+= 1.0/20)
            {
                // fixed x = lerp(-_FeatherXRange, _FeatherXRange , frac(i+speed *0.1));
                // fixed y = lerp(-_FeatherYRange, _FeatherYRange , frac(sin(i *564.3)*498.38));
                fixed x = lerp(-3, 3, frac(i+speed *0.1));
                fixed y = lerp(1.25, 3, frac(sin(i *564.3)*498.38));
                fixed z = lerp( 3.0, 0.0, i);

                fixed4 feather = FeatherBall(ro, rd , fixed3(x,y,z), speed + i*563.34);
                feather.rgb = sqrt(feather.rgb) * fixed4(0.4435955, 0.4435955, 0.56, 1);
                col = lerp(col , feather, feather.a);
            }

            col = pow(col , 0.456); // gamma correction

            return col;
        }
        ENDCG
    }
}


CustomEditor "SkyboxPanoramicShaderGUI"
Fallback Off

}
