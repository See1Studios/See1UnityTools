Shader "See1View/HeightFog"
{
    Properties
    {
        _Height ("Height", Float) = 2
        _Ground ("Ground", Float) = 0
        _Color ("Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        
        Pass
        {
			ColorMask RGB
			Blend SrcAlpha  OneMinusSrcAlpha
			//Blend Zero SrcColor
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex: POSITION;
            };

            struct v2f
            {
                float4 vertex: SV_POSITION;
                float3 worldPos: TEXCOORD0;
            };

            fixed _Height;
            fixed _Ground;
            fixed4 _Color;

            // remap value to 0-1 range
            float remap(float value, float minSource, float maxSource)
            {
                return(value - minSource) / (maxSource - minSource);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag(v2f i): COLOR
            {
                fixed4 c = fixed4(0, 0, 0, 0);
                float bottom = _Ground;
                float top = _Ground + _Height;
                float v = remap(clamp(i.worldPos.y, bottom, top), bottom, top);
                fixed4 t = fixed4(0,0,0,0);
				c = lerp(_Color, t, v);
                return c;
            }
            ENDCG
            
        }
    }
}