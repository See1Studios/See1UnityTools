// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//Shader "SuperView/NormalVisualize"
//{
//	Properties
//	{
//		_BumpMap("Normal Map", 2D) = "bump" {}
//	}
//	SubShader
//	{
//		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
//		// No culling or depth
//		Cull Off ZWrite Off ZTest Always

//		Pass
//		{
			
//			ZWrite On
//			ZTest LEqual 
//			Blend SrcAlpha  OneMinusSrcAlpha
			
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
			
//			#include "UnityCG.cginc"

//			sampler2D _BumpMap;

//            struct v2f {
//                float3 worldPos : TEXCOORD0;
//                // these three vectors will hold a 3x3 rotation matrix
//                // that transforms from tangent to world space
//                half3 tspace0 : TEXCOORD1; // tangent.x, bitangent.x, normal.x
//                half3 tspace1 : TEXCOORD2; // tangent.y, bitangent.y, normal.y
//                half3 tspace2 : TEXCOORD3; // tangent.z, bitangent.z, normal.z
//                // texture coordinate for the normal map
//                float2 uv : TEXCOORD4;
//                float4 pos : SV_POSITION;
//            };

//            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL, float4 tangent : TANGENT, float2 uv : TEXCOORD0)
//            {
//                v2f o;
//                o.pos = UnityObjectToClipPos(vertex);
//                o.worldPos = mul(unity_ObjectToWorld, vertex).xyz;
//                half3 wNormal = UnityObjectToWorldNormal(normal);
//                half3 wTangent = UnityObjectToWorldDir(tangent.xyz);
//                // compute bitangent from cross product of normal and tangent
//                half tangentSign = tangent.w * unity_WorldTransformParams.w;
//                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
//                // output the tangent space matrix
//                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
//                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
//                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
//                o.uv = uv;
//                return o;
//            }

//			fixed4 frag (v2f i) : SV_Target
//			{                
//				// sample the normal map, and decode from the Unity encoding
//                half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.uv));
//                // transform normal from tangent to world space
//                half3 worldNormal;
//                worldNormal.x = dot(i.tspace0, tnormal);
//                worldNormal.y = dot(i.tspace1, tnormal);
//                worldNormal.z = dot(i.tspace2, tnormal);
//				return float4(worldNormal,1);
//			}
//			ENDCG
//		}
//	}
//}
//Shader "SuperView/NormalVisualize"
//{
//Properties {
//	_MainTex ("Base (RGB)", 2D) = "white" {}
//	_RampTex ("Base (RGB)", 2D) = "grayscaleRamp" {}
//}

//SubShader {
//	Pass {
//		ZTest Always Cull Off ZWrite Off
				
//CGPROGRAM
//#pragma vertex vert_img
//#pragma fragment frag
//#include "UnityCG.cginc"

//uniform sampler2D _MainTex;
//uniform sampler2D _RampTex;
//uniform half _RampOffset;

//fixed4 frag (v2f_img i) : SV_Target
//{
//	fixed4 original = tex2D(_MainTex, i.uv);
//	fixed grayscale = Luminance(original.rgb);
//	half2 remap = half2 (grayscale + _RampOffset, .5);
//	fixed4 output = tex2D(_RampTex, remap);
//	output.a = original.a;
//	return output;
//}
//ENDCG

//	}
//}

//Fallback off

//}
Shader "See1View/DepthNormal"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _Seperate ("Seperate", range(0, 1)) = 0.5
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _CameraDepthNormalsTexture;
			float4 _CameraDepthNormalsTexture_TexelSize;
            half _Seperate;

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}			

			fixed4 frag (v2f i) : SV_Target
			{
                float4 col = float4(1, 0, 0, 1);
				if (i.vertex.x > _CameraDepthNormalsTexture_TexelSize.z / (1 / _Seperate))
                {
					fixed3 tex = tex2D(_MainTex, i.uv).rgb;
					fixed4 dn = tex2D(_CameraDepthNormalsTexture, i.uv);
					float depth;
					float3 normal;
					DecodeDepthNormal(dn, depth, normal);
					//fixed grayscale = Luminance(tex.rgb);
					//return float4(grayscale,grayscale,grayscale, 1);
					col = float4(normal, 1);
				}                
				else
                {
                    col = tex2D(_MainTex, i.uv);
                }
                return col;
			}
			ENDCG
		}
	}
}
//Shader "SuperView/NormalVisualize"
//{
//	Properties
//	{
//		_Color ("Color", Color) = (1,0,0,0)
//		_SrcBlend ("SrcBlend", Int) = 5.0 // SrcAlpha
//		_DstBlend ("DstBlend", Int) = 10.0 // OneMinusSrcAlpha
//		_ZWrite ("ZWrite", Int) = 1.0 // On
//		_ZTest ("ZTest", Int) = 4.0 // LEqual
//		_Cull ("Cull", Int) = 0.0 // Off
//		_ZBias ("ZBias", Float) = 0.0
//	}

//	SubShader
//	{
//		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
//		Pass
//		{
//			Blend [_SrcBlend] [_DstBlend]
//			ZWrite [_ZWrite]
//			ZTest [_ZTest]
//			Cull [_Cull]
//			Offset [_ZBias], [_ZBias]

//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//			struct appdata_t {
//				float4 vertex : POSITION;
//				float4 color : COLOR;
//			};
//			struct v2f {
//				fixed4 color : COLOR;
//				float4 vertex : SV_POSITION;
//			};
//			float4 _Color;
//			v2f vert (appdata_t v)
//			{
//				v2f o;
//				o.vertex = UnityObjectToClipPos(v.vertex);
//				o.color = v.color * _Color;
//				return o;
//			}
//			fixed4 frag (v2f i) : SV_Target
//			{
//				return i.color;
//			}
//			ENDCG  
//		}  
//	}
//}