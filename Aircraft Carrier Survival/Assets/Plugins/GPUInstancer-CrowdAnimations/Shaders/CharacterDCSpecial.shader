
Shader "CharacterDCSpecial" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MainTex2 ("Metalic (RGB)", 2D) = "white" {}
		_MainTex3 ("Normal (RGB)", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 400

		CGPROGRAM
		#include "UnityCG.cginc"
		#include "Include/GPUICrowdInclude.cginc"
		#pragma instancing_options procedural:setupGPUI
		#pragma multi_compile_instancing
		#pragma surface surf Standard addshadow fullforwardshadows vertex:colorVariationVert  

		#pragma target 5.0

		sampler2D _MainTex;
		sampler2D _MainTex2;
		sampler2D _MainTex3;
		struct Input {
			float2 uv_MainTex;
			float4 colorVariation;
			float4 color: Color;
			
		};

		half _Glossiness;
	//	half _Metallic;
		fixed4 _Color;

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float4> colorBuffer;
		#endif
		
		void colorVariationVert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);

			o.colorVariation = _Color;

			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				o.colorVariation = colorBuffer[gpui_InstanceID];
			#endif
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) ;
			fixed4 m = tex2D (_MainTex2, IN.uv_MainTex) ;//* saturate(IN.colorVariation);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = m.w;
			o.Smoothness = m.x;
			
			float a= m.z*IN.colorVariation.r;
			clip (-a);
		}
		ENDCG
	}
	FallBack "Standard"
}
