Shader "Hidden/EdgeThicken"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OPosTex ("object position", 2D) = "black"{}
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
			#define ts _OPosTex_TexelSize

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
			
			sampler2D _MainTex, _OPosTex;
			half4 _OPosTex_TexelSize;

			half4 frag (v2f i) : SV_Target
			{
				half alpha = tex2D(_OPosTex, i.uv).a;
				half edgeThick = 8.0;

				half a00 = tex2D(_OPosTex, i.uv + edgeThick * float2(-ts.x,-ts.y)).a;
				half a01 = tex2D(_OPosTex, i.uv + edgeThick * float2(-ts.x, ts.y)).a;
				half a10 = tex2D(_OPosTex, i.uv + edgeThick * float2( ts.x,-ts.y)).a;
				half a11 = tex2D(_OPosTex, i.uv + edgeThick * float2( ts.x, ts.y)).a;
				half2 dir = half2(a10+a11-a00-a01, a01+a11-a00-a10);

				half4 col = tex2D(_MainTex, i.uv);
				half4 col1 = tex2D(_MainTex, i.uv + normalize(dir) * ts.xy * edgeThick);
				
				if(alpha < 1)
					col = col1;
				
//				return tex2D(_OPosTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
