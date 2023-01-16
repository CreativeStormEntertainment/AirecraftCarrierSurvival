// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MasterShader"
{
	Properties
	{
		_MainTex("_MainTex", 2DArray ) = "" {}
		_BumpMap("_BumpMap", 2DArray ) = "" {}
		_MainTex2("_MainTex", 2DArray ) = "" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.5
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform UNITY_DECLARE_TEX2DARRAY( _BumpMap );
		uniform UNITY_DECLARE_TEX2DARRAY( _MainTex );
		uniform UNITY_DECLARE_TEX2DARRAY( _MainTex2 );

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float clampResult6 = clamp( ( i.vertexColor.a * 255.0 ) , 0.0 , 255.0 );
			float3 texArray7 = UnpackNormal( UNITY_SAMPLE_TEX2DARRAY(_BumpMap, float3(i.uv_texcoord, clampResult6)  ) );
			o.Normal = float4( texArray7 , 0.0 );
			float4 texArray1 = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.uv_texcoord, clampResult6)  );
			o.Albedo = texArray1.rgb * i.vertexColor.rgb;
			float4 texArray8 = UNITY_SAMPLE_TEX2DARRAY(_MainTex2, float3(i.uv_texcoord, clampResult6)  );
			o.Metallic = texArray8.r;
			o.Smoothness = texArray8.a;
			o.Occlusion = texArray8.g;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=17700
0;0;1920;1019;1189.48;518.6063;1.093199;True;True
Node;AmplifyShaderEditor.VertexColorNode;4;-670,-329.5;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-478,-293.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;255;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-654.314,-49.71075;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;6;-335,-305.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;255;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-588,-525.5;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-610.2512,271.3874;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureArrayNode;7;-232.8441,-83.4678;Inherit;True;Property;_BumpMap;_BumpMap;1;0;Create;False;0;0;False;0;None;0;Object;-1;Auto;True;7;6;SAMPLER2D;;False;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.UnpackScaleNormalNode;11;96.03532,-38.68585;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureArrayNode;8;-232.8027,244.7016;Inherit;True;Property;_MainTex2;_MainTex;2;0;Create;True;0;0;False;0;None;0;Object;-1;Auto;False;7;6;SAMPLER2D;;False;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureArrayNode;1;31,-392.5;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;False;0;None;0;Object;-1;Auto;False;7;6;SAMPLER2D;;False;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;378.6684,-86.56577;Float;False;True;-1;3;ASEMaterialInspector;0;0;Standard;MasterShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;4;1
WireConnection;6;0;5;0
WireConnection;7;0;9;0
WireConnection;7;1;6;0
WireConnection;11;0;7;0
WireConnection;8;0;10;0
WireConnection;8;1;6;0
WireConnection;1;0;3;0
WireConnection;1;1;6;0
WireConnection;0;0;1;0
WireConnection;0;1;11;0
WireConnection;0;3;8;1
WireConnection;0;4;8;4
WireConnection;0;5;8;2
ASEEND*/
//CHKSM=426AD3B130EF4B7F3BB32CDAB53413FBBC3EF403