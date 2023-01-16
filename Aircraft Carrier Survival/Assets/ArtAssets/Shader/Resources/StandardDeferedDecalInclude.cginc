#include "UnityStandardCore.cginc"
half        _Overal_Alpha;
half        _Diffuse_Alpha;
half        _Normal_Alpha;
half        _Specular_Alpha;
half        _Smoothness_Alpha;
half        _Occlusion_Alpha;

//$$$------
void fragDeferredDull(
	VertexOutputDeferred i,
	out half4 outGBuffer0 : SV_Target0,
	out half4 outGBuffer1 : SV_Target1,
	out half4 outGBuffer2 : SV_Target2,
	out half4 outEmission : SV_Target3          // RT3: emission (rgb), --unused-- (a)
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	, out half4 outShadowMask : SV_Target4       // RT4: shadowmask (rgba)
#endif
)
{
//#if (SHADER_TARGET < 30)
//	outGBuffer0 = 1;
//	outGBuffer1 = 1;
//	outGBuffer2 = 0;
//	outEmission = 0;
//#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
//	outShadowMask = 1;
//#endif
//	return;
//#endif

	//UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

	FRAGMENT_SETUP(s)
//
//		// no analytic lights in this pass
//		UnityLight dummyLight = DummyLight();
//	half atten = 1;
//
//	// only GI
//	half occlusion = Occlusion(i.tex.xy);
//#if UNITY_ENABLE_REFLECTION_BUFFERS
//	bool sampleReflectionsInDeferred = false;
//#else
//	bool sampleReflectionsInDeferred = true;
//#endif
//
//	UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);
//
//	half3 emissiveColor = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;
//
//#ifdef _EMISSION
//	emissiveColor += Emission(i.tex.xy);
//#endif
//
//#ifndef UNITY_HDR_ON
//	emissiveColor.rgb = exp2(-emissiveColor.rgb);
//#endif
//
	half alpha = Alpha(i.tex.xy)*_Overal_Alpha;

	outGBuffer0 = half4(1 - alpha*_Diffuse_Alpha, 1 - alpha*_Diffuse_Alpha, 1 - alpha*_Diffuse_Alpha, 1 - alpha*_Occlusion_Alpha);
	outGBuffer1 = half4(1 - alpha*_Specular_Alpha, 1 - alpha*_Specular_Alpha, 1 - alpha*_Specular_Alpha, 1 - alpha*_Smoothness_Alpha);
	outGBuffer2 = half4(1 - alpha*_Normal_Alpha, 1 - alpha*_Normal_Alpha, 1 - alpha*_Normal_Alpha, 1 - alpha);

	// Emissive lighting buffer
	//outEmission = half4(emissiveColor, 1);

	outEmission = half4(1 - alpha*_Diffuse_Alpha, 1 - alpha*_Diffuse_Alpha, 1 - alpha*_Diffuse_Alpha, 1 - alpha*_Diffuse_Alpha);

	// Baked direct lighting occlusion if any
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	//outShadowMask = UnityGetRawBakedOcclusions(i.ambientOrLightmapUV.xy, IN_WORLDPOS(i));

	outShadowMask = half4(1 - alpha, 1 - alpha, 1 - alpha, 1 - alpha);
#endif
}

void fragDeferredPremultAlpha(
    VertexOutputDeferred i,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3          // RT3: emission (rgb), --unused-- (a)
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    ,out half4 outShadowMask : SV_Target4       // RT4: shadowmask (rgba)
#endif
)
{
    #if (SHADER_TARGET < 30)
        outGBuffer0 = 1;
        outGBuffer1 = 1;
        outGBuffer2 = 0;
        outEmission = 0;
        #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
            outShadowMask = 1;
        #endif
        return;
    #endif

    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    FRAGMENT_SETUP(s)

    // no analytic lights in this pass
    UnityLight dummyLight = DummyLight ();
    half atten = 1;

    // only GI
    half occlusion = Occlusion(i.tex.xy);
#if UNITY_ENABLE_REFLECTION_BUFFERS
    bool sampleReflectionsInDeferred = false;
#else
    bool sampleReflectionsInDeferred = true;
#endif

    UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);

    half3 emissiveColor = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;

    #ifdef _EMISSION
        emissiveColor += Emission (i.tex.xy);
    #endif

    #ifndef UNITY_HDR_ON
        emissiveColor.rgb = exp2(-emissiveColor.rgb);
    #endif


    UnityStandardData data;
	data.diffuseColor = s.diffColor;
	data.occlusion = occlusion;
	data.specularColor = s.specColor;
	data.smoothness = s.smoothness;
	data.normalWorld = s.normalWorld;

    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);
	//$$$$----------
	half alpha = Alpha(i.tex.xy)*_Overal_Alpha;
	outGBuffer0 = outGBuffer0*half4(alpha*_Diffuse_Alpha, alpha*_Diffuse_Alpha, alpha*_Diffuse_Alpha, alpha*_Occlusion_Alpha);
	outGBuffer1 = outGBuffer1*half4(alpha*_Specular_Alpha, alpha*_Specular_Alpha, alpha*_Specular_Alpha, alpha*_Smoothness_Alpha);
	outGBuffer2 = outGBuffer2*half4(alpha*_Normal_Alpha, alpha*_Normal_Alpha, alpha*_Normal_Alpha, alpha);
	//$$$$----------

    // Emissive lighting buffer
    outEmission = half4(emissiveColor, 1)
		//$$$$----------
		*half4(alpha*_Diffuse_Alpha, alpha*_Diffuse_Alpha, alpha*_Diffuse_Alpha, alpha*_Occlusion_Alpha);
		//$$$$----------

    // Baked direct lighting occlusion if any
    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
        outShadowMask = UnityGetRawBakedOcclusions(i.ambientOrLightmapUV.xy, IN_WORLDPOS(i))
			//$$$$----------
			*alpha;
			//$$$$----------
    #endif
}
//$$$------