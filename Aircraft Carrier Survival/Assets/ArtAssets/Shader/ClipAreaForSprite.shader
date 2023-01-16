Shader "ClipAreaForSprite"
{

    Properties
    {
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_MainTex2 ("B, Alpha (A)", 2D) = "white" {}
		_MainTex3 ("Icon, Alpha (A)", 2D) = "white" {}
        _Fill ("Fill Current", Range(0.0, 1.0)) = 1.0
        _MinX ("MinX", Float) = 0
        _MaxX ("MaxX", Float) = 1
     }

    SubShader
    {
        LOD 200

        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Cull Off 
            Lighting Off
            ZWrite Off
            Offset -1, -1
            Fog { Mode Off }
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
			 sampler2D _MainTex2;
			  sampler2D _MainTex3;
            float4 _MainTex_ST;
            float _MinX;
            float _MaxX;
            float _Fill;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0; 
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            half4 frag (v2f IN) : COLOR 
            {

			float4 col =  tex2D(_MainTex, IN.texcoord);
			float4 col2= tex2D(_MainTex2, IN.texcoord);
			float4 col3= tex2D(_MainTex3, IN.texcoord);
            if ((IN.texcoord.y<_MinX)|| (IN.texcoord.y>(_MinX+_Fill*(_MaxX-_MinX))))
            {
                 col2.rgb = col;
                 
            }
            else
				col2 = col2;

			col.rgb-=col2*col2.a;
			
			col.rgb+= col3.rgb*col3.a;
			return col;
            }
            ENDCG
        }
    }
} 
