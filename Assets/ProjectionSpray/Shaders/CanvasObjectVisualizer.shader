Shader "Unlit/CanvasObjectVisualizer"
{
	Properties
	{
		_Canvas ("canvas", 2D) = "gray"{}
		_Guid ("guid", 2D) = "black"{}
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
			#include "Assets/CGINC/ColorCollect.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _Canvas, _Guid;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv2;
				o.normal = v.normal;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				half4 col = tex2D(_Canvas, i.uv);
				half4 guid = tex2D(_Guid, i.uv);

				half4 overlay = blendOverlay(col, guid);
				half4 screen = blendScreen(col, guid);

				half4 blend = abs(col-overlay) < abs(col-screen) ? screen : overlay;

				return lerp(col, blend, guid.a);
			}
			ENDCG
		}
	}
}
