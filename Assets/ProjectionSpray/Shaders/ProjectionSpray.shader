Shader "Hidden/Projectionspray"
{
	Properties
	{
		_MainTex ("canvas", 2D) = "white" {}
		_Shape ("splay shape", 2D) = "black"{}
		_Depth ("projecor depth", 2D) = "black"{}
		
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
			
		sampler2D _MainTex, _Shape, _OPosTex, _ONormTex, _Depth;
		float4x4 _ProjectionMatrix, _MatrixO2W, _MatrixW2D;
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

		float calcAtten(float3 vPos, float3 vNorm, inout float depth){
			depth = length(vPos);
			float3 dir = normalize(vPos);
			float atten = max(0, dot(vNorm, -dir));
			return atten;
		}

		//MatrixW2D: world to drawer matrix.
		half4 projectedTex(half3 wPos, half3 wNorm)
		{
			float4 uvProj = mul(_ProjectionMatrix, float4(wPos,1));
			half2 uv = uvProj.xy/uvProj.w;

			half4 tex = tex2Dlod (_Shape, half4(uv,0,0));

			float3 vPos = mul(_MatrixW2D, float4(wPos,1)).xyz;
			float3 vNorm = mul((float3x3)_MatrixW2D, wNorm);
			float depth;
			float depthMask = tex2Dlod(_Depth, half4(uv,0,0)).r;
			float atten = calcAtten(vPos, vNorm, depth);

			tex.a *= 0 < uvProj.z;
			tex.a *= (abs(uv.x-0.5)<0.5) * (abs(uv.y-0.5)<0.5);
			tex.a *= 1.0 - saturate(( depth - depthMask)*100.0);
			tex.a *= atten;
			tex.a *= lerp(1, 0, (depth/_Dst));

			return tex;
		}

		half4 draw (v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv);
			half3 wPos, wNorm;
			getObjectPos(i.uv, wPos, wNorm);
			half4 tex = projectedTex(wPos, wNorm) * _Color;
			tex.a *= unity_DeltaTime * _Emission;

			col.rgb = lerp(col.rgb, tex.rgb, tex.a);
			col.a = 1;
			
			return col;
		}

		half4 drawGuid(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv);
			half3 wPos, wNorm;
			getObjectPos(i.uv, wPos, wNorm);
			half4 tex = projectedTex(wPos, wNorm) * _Color;

			col = lerp(col, tex, tex.a);
			col.a = tex.a;
			
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
