
Shader "WorkerShader" {
	Properties {
		_MainTex("Albedo", 2D) = "white" {}
		_MetallicGlossMap("Metallic", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_OcclusionMap("Occlusion", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 400

		CGPROGRAM
		#include "UnityCG.cginc"
		#include "../../Plugins/GPUInstancer-CrowdAnimations/Shaders/Include/GPUICrowdInclude.cginc"
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setupGPUI
		#pragma surface surf Standard addshadow fullforwardshadows vertex:vert  

		#pragma target 5.0

		sampler2D _MainTex;
		sampler2D _MetallicGlossMap;
		sampler2D _BumpMap;
		sampler2D _OcclusionMap;
		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float colorSaturation;
		};

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<float> saturationBuffer;
#endif
		
		void vert(inout appdata_full v, out Input o)
		{
			GPUI_CROWD_VERTEX(v);
			UNITY_INITIALIZE_OUTPUT(Input, o);

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			o.colorSaturation = saturationBuffer[gpui_InstanceID];
#else
			o.colorSaturation = 1.0;
#endif
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * IN.colorSaturation;
			o.Albedo = color.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

			fixed4 metallic = tex2D(_MetallicGlossMap, IN.uv_MainTex);
			o.Metallic = metallic.r;
			o.Smoothness = metallic.a;

			fixed4 occlusion = tex2D(_OcclusionMap, IN.uv_MainTex);
			o.Occlusion = occlusion.g;
		}
		ENDCG
	}
	FallBack "Standard"
}
