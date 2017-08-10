Shader "Unlit/DepthRenderer"
{
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

			struct v2f
			{
				float depth : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (float4 vertex : POSITION)
			{
				float4 wPos = mul(unity_ObjectToWorld, vertex);
				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.depth = length(mul(UNITY_MATRIX_V, wPos).xyz);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				return i.depth;
			}
			ENDCG
		}
	}
}
