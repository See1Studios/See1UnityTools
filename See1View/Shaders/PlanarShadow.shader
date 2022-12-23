Shader "See1View/PlanarShadow" 
{

	Properties {
		_ShadowColor ("Shadow Color", Color) = (0,0,0,1)
		_PlaneHeight ("Plane Height", Float) = 0
	}

	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		
		// shadow color
		Pass {   
			
			ZWrite On
			ZTest LEqual 
			Blend SrcAlpha  OneMinusSrcAlpha
			
			Stencil {
				Ref 0
				Comp Equal
				Pass IncrWrap
				ZFail Keep
			}

			CGPROGRAM
			#include "UnityCG.cginc"

			// User-specified uniforms
			uniform float4 _ShadowColor;
			uniform float _PlaneHeight = 0;

			struct vsOut
			{
				float4 pos	: SV_POSITION;
			};

			vsOut vertPlanarShadow( appdata_base v)
			{
				vsOut o;
         	            
				float4 vPosWorld = mul( unity_ObjectToWorld, v.vertex);
				float4 lightDirection = -normalize(_WorldSpaceLightPos0); 

				float opposite = vPosWorld.y - _PlaneHeight;
				float cosTheta = -lightDirection.y;	// = lightDirection dot (0,-1,0)
				float hypotenuse = opposite / cosTheta;
				float3 vPos = vPosWorld.xyz + ( lightDirection * hypotenuse );

				o.pos = mul (UNITY_MATRIX_VP, float4(vPos.x, _PlaneHeight, vPos.z ,1));  
	
				return o;
			}

			float4 fragPlanarShadow( vsOut i)
			{
				return _ShadowColor;
			}
			#pragma vertex vert
			#pragma fragment frag

			vsOut vert( appdata_base v)
			{
				return vertPlanarShadow(v);
			}


			fixed4 frag( vsOut i) : COLOR
			{
				return fragPlanarShadow(i);
			}

			ENDCG

		}
	}
}
