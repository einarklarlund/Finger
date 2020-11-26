 Shader "Unlit/CubemapTestBaked"
 {
     Properties
     {
         _MainTex ("Texture", 2DArray) = "white" {}
     }
     SubShader
     {
         Tags { "RenderType"="Opaque" }
         Cull Off
         LOD 100
 
         Pass
         {
             CGPROGRAM
 
             #pragma vertex vert
             #pragma fragment frag
             
             #include "UnityCG.cginc"
 
             // Includes xyz_to_uvw, uvw_to_xyz, xyz_to_side, xyz_to_uvw_force_side based on a macro for shaders
             // But we don't need this since we've baked in the Tex2DArray sample coords in C# script
             // #include "CubemapTransform.cs"
 
             struct appdata
             {
                 float4 vertex : POSITION;
 
                 // Note: Not actually normal, we just stole its semantic
                 float4 bakedSampleCoord : NORMAL;
             };
 
             struct v2f
             {
                 float4 vertex : SV_POSITION;
 
                 // Custom interpolators
                 float4 bakedSampleCoord : TEXCOORD0;
             };
 
             UNITY_DECLARE_TEX2DARRAY(_MainTex);
             
             v2f vert (appdata v)
             {
                 v2f o;
                 o.bakedSampleCoord = v.bakedSampleCoord;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 return o;
             }
             
             fixed4 frag (v2f i) : SV_Target
             {
                 // Note, we don't have to call xyz_to_uvw() when we have it baked
                 // Which makes our frag shader quite faster (saves around 15 instructions)
                 return UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.bakedSampleCoord);
             }
 
             ENDCG
         }
     }
 }