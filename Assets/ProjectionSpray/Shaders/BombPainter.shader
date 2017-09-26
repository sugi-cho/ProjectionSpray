Shader "Hidden/BombPainter"
{
	Properties
	{
		_MainTex ("canvas", 2D) = "white" {}
		_Depth ("projecor depth", Cube) = "black"{}
		
		_OPosTex ("object position", 2D) = "black"{}
		_ONormTex ("object normal", 2D) = "black"{}
		
		_Color("draw color", Color) = (1,0,0,0)
		_Pos("position", Vector) = (0,0,0,0)
		_Rot("rotation", Vector) = (0,0,0,0)
		_Emission("emission per sec", Float) = 1
		_Dst ("max distance", Float) = 2
	}
	CGINCLUDE
		#include "UnityCG.cginc"

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
		
		samplerCUBE _Depth;
		sampler2D _MainTex, _Shape, _OPosTex, _ONormTex;
		float4x4 _MatrixO2W;
		half4 _Color, _Rot;
		half3 _Pos;
		half _Emission, _Dst;

		void getObjectPos(float2 uv, inout half3 wPos, inout half3 wNorm){
			half4 pos = tex2D(_OPosTex, uv);
			half3 norm = tex2D(_ONormTex, uv).xyz;
			wPos = mul(_MatrixO2W, pos);
			wNorm = normalize(mul((float3x3)_MatrixO2W, norm));
			wPos -= (pos.a < 0.5) * 10000;
		}

		float calcAtten(float3 to, float3 wNorm){
			float3 dir = normalize(to);
			float atten = max(0, dot(wNorm, -dir));
			return atten;
		}

		half projectedVal(half3 wPos, half3 wNorm)
		{
			float3 to = wPos - _Pos;
			float depth = length(to);
			float depthMask = texCUBE(_Depth, normalize(to)).r;
			float atten = calcAtten(to, wNorm);

			half val = 1.0;

			val *= 1.0 - saturate(( depth - depthMask)*100.0);
			val *= atten;
			val *= lerp(1, 0, saturate(depth/_Dst));

			return val;
		}

		half4 draw (v2f i) : SV_Target
		{
			fixed4 col0 = tex2D(_MainTex, i.uv);
			half3 wPos, wNorm;
			getObjectPos(i.uv, wPos, wNorm);

			half4 col1 = _Color;
			col1.a *= projectedVal(wPos, wNorm);
			col1.a *= unity_DeltaTime * _Emission;

			half4 col =lerp(col0, col1, col1.a);
			col.a = 1;
			
			return col;
		}

		half4 drawGuid(v2f i) : SV_Target
		{
			fixed4 col0 = tex2D(_MainTex, i.uv);
			half3 wPos, wNorm;
			getObjectPos(i.uv, wPos, wNorm);

			half4 col1 = _Color;
			col1.a *= projectedVal(wPos, wNorm);
			
			half4 col =lerp(col0, col1, col1.a);
			col.a = col1.a;
			
			return col;
		}
	ENDCG
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment draw
			
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment drawGuid
			
			ENDCG
		}
	}
}
