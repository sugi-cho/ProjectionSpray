Shader "Hidden/SprayPaintTexGen"
{
	Properties
	{
		_Edge ("soft edge", Float) = 0.1
		_Freq ("frequency of noise", Float) = 10
		_Amp ("amplitude of noise", Float) = 0.1
	}
	SubShader
	{
		Tags {"PreviewType" = "Plane"}
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Assets/CGINC/SimplexNoise2D.cginc"

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
			
			float _Edge,_Freq,_Amp;

			fixed4 frag (v2f i) : SV_Target
			{
				half2 xy = i.uv - 0.5;
				half d = length(xy);
				half noise = snoise(normalize(xy)*_Freq);
				half shape = 0.5 - _Amp + noise * _Amp;
				
				half alpha = smoothstep(shape, shape * (1.0 - _Edge), d);
				return half4(1,1,1,alpha);
			}
			ENDCG
		}
	}
}
