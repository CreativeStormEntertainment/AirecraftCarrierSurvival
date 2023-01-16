Shader "Custom/GrayscaleShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}

		Cull Off

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;

				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1,1);
				#endif

				OUT.color = IN.color;
				return OUT;
			}

			fixed4 _TextureSampleAdd;

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
				color.rgb = lerp(color.rgb, dot(color.rgb, float3(0.3, 0.59, 0.11)), IN.color.r);
				return color;
			}
		ENDCG
		}
	}
	FallBack "UI/Default"
}