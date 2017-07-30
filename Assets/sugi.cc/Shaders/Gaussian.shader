Shader "Hidden/Gaussian"
{
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
	}
	SubShader {
		ZTest Always ZWrite Off Cull Off Fog { Mode Off }
		
			CGINCLUDE
			const static float WEIGHTS[8] = {  0.013,  0.067,  0.194,  0.226, 0.226, 0.194, 0.067, 0.013 };
			const static float OFFSETS[8] = { -6.264, -4.329, -2.403, -0.649, 0.649, 2.403, 4.329, 6.264 };
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			struct vsin {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct vs2psDown {
				float4 vertex : POSITION;
				float2 uv[4] : TEXCOORD0;
			};
			struct vs2psBlur {
				float4 vertex : POSITION;
				float2 uv[8] : TEXCOORD0;
			};
			
			vs2psDown vertDownsample(vsin IN) {
				vs2psDown OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv[0] = IN.uv + float2( 0.5,  0.5) * _MainTex_TexelSize.xy;
				OUT.uv[1] = IN.uv + float2(-0.5, -0.5) * _MainTex_TexelSize.xy;
				OUT.uv[2] = IN.uv + float2( 0.5, -0.5) * _MainTex_TexelSize.xy;
				OUT.uv[3] = IN.uv + float2(-0.5,  0.5) * _MainTex_TexelSize.xy;
				return OUT;
			}
			float4 fragDownsample(vs2psDown IN) : COLOR {
				float4 c = 0;
					c += tex2D(_MainTex, IN.uv[0]) * 0.25;
					c += tex2D(_MainTex, IN.uv[1]) * 0.25;
					c += tex2D(_MainTex, IN.uv[2]) * 0.25;
					c += tex2D(_MainTex, IN.uv[3]) * 0.25;
				return c;
			}
			
			vs2psBlur vertBlurH(vsin IN) {
				vs2psBlur OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.uv[0] = IN.uv + float2(OFFSETS[0], 0) * _MainTex_TexelSize.xy;
					OUT.uv[1] = IN.uv + float2(OFFSETS[1], 0) * _MainTex_TexelSize.xy;
					OUT.uv[2] = IN.uv + float2(OFFSETS[2], 0) * _MainTex_TexelSize.xy;
					OUT.uv[3] = IN.uv + float2(OFFSETS[3], 0) * _MainTex_TexelSize.xy;
					OUT.uv[4] = IN.uv + float2(OFFSETS[4], 0) * _MainTex_TexelSize.xy;
					OUT.uv[5] = IN.uv + float2(OFFSETS[5], 0) * _MainTex_TexelSize.xy;
					OUT.uv[6] = IN.uv + float2(OFFSETS[6], 0) * _MainTex_TexelSize.xy;
					OUT.uv[7] = IN.uv + float2(OFFSETS[7], 0) * _MainTex_TexelSize.xy;
				return OUT;
			}
			vs2psBlur vertBlurV(vsin IN) {
				vs2psBlur OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.uv[0] = IN.uv + float2(0, OFFSETS[0]) * _MainTex_TexelSize.xy;
					OUT.uv[1] = IN.uv + float2(0, OFFSETS[1]) * _MainTex_TexelSize.xy;
					OUT.uv[2] = IN.uv + float2(0, OFFSETS[2]) * _MainTex_TexelSize.xy;
					OUT.uv[3] = IN.uv + float2(0, OFFSETS[3]) * _MainTex_TexelSize.xy;
					OUT.uv[4] = IN.uv + float2(0, OFFSETS[4]) * _MainTex_TexelSize.xy;
					OUT.uv[5] = IN.uv + float2(0, OFFSETS[5]) * _MainTex_TexelSize.xy;
					OUT.uv[6] = IN.uv + float2(0, OFFSETS[6]) * _MainTex_TexelSize.xy;
					OUT.uv[7] = IN.uv + float2(0, OFFSETS[7]) * _MainTex_TexelSize.xy;
				return OUT;
			}
			float4 fragBlur(vs2psBlur IN) : COLOR {
				float4 c = 0;
					c += tex2D(_MainTex, IN.uv[0]) * WEIGHTS[0];
					c += tex2D(_MainTex, IN.uv[1]) * WEIGHTS[1];
					c += tex2D(_MainTex, IN.uv[2]) * WEIGHTS[2];
					c += tex2D(_MainTex, IN.uv[3]) * WEIGHTS[3];
					c += tex2D(_MainTex, IN.uv[4]) * WEIGHTS[4];
					c += tex2D(_MainTex, IN.uv[5]) * WEIGHTS[5];
					c += tex2D(_MainTex, IN.uv[6]) * WEIGHTS[6];
					c += tex2D(_MainTex, IN.uv[7]) * WEIGHTS[7];
				return c;
			}
			ENDCG		
		
		// 0 : Downsample
		Pass {
			CGPROGRAM
			#pragma vertex vertDownsample
			#pragma fragment fragDownsample
			#pragma target 3.0
			ENDCG
		}
		// 1 : Horizontal Separable Gaussian
		Pass {
			CGPROGRAM
			#pragma vertex vertBlurH
			#pragma fragment fragBlur
			#pragma target 3.0
			ENDCG
		}
		// 2 : Vertical Separable Gaussian
		Pass {
			CGPROGRAM
			#pragma vertex vertBlurV
			#pragma fragment fragBlur
			#pragma target 3.0
			ENDCG
		}
	} 
	FallBack Off
}