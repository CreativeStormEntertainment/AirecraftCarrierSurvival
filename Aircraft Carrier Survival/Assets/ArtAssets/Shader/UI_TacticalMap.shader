Shader "Unlit/UI_TacticalMap"
{
    Properties
    {
        [PerRendererData] _MainTex("Texture", 2D) = "white" {}
        _PatternTex("Image_01", 2D) = "white" {}
        _PatternTex2("Image_02", 2D) = "white" {}
        //_PatternTex4("Image_01_Cloud", 2D) = "white" {}
        //_PatternTex5("Image_02_Cloud", 2D) = "white" {}
        _PatternTex3("Mask Texture", 2D) = "white" {}
        //_SpeedX("Speed X", Range(-1, 1)) = 0
        //_SpeedY("Speed Y", Range(-1, 1)) = 0
        //_CloudsVisible("Clouds Visible", Range(0, 1)) = 1
    }
        SubShader
        {
            Tags
            {
                    "Queue" = "Transparent"
                    "RenderType" = "Transparent"
                    "PreviewType" = "Plane"
                    "CanUseSpriteAtlas" = "True"
            }
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex	: POSITION;
                    float2 uv		: TEXCOORD0;
                    float4 color	: COLOR;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    half4 color : COLOR;
                    half2 patternUV : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                sampler2D _PatternTex;
                sampler2D _PatternTex2;
                sampler2D _PatternTex3;
                //sampler2D _PatternTex4;
                //sampler2D _PatternTex5;
                float4 _PatternTex_ST;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.patternUV = TRANSFORM_TEX(v.uv, _PatternTex);
                    o.color = v.color;
                    return o;
                }

                //half _SpeedX;
                //half _SpeedY;
                //half _CloudsVisible;

                half4 frag(v2f i) : SV_Target
                {
                    half4 col = lerp(tex2D(_PatternTex, i.uv),tex2D(_PatternTex2, i.uv), 1.0 - tex2D(_PatternTex3, i.uv));
                    //float2 offset = frac(_Time.y * float2(_SpeedX, _SpeedY));
                    //half4 col2 = lerp(tex2D(_PatternTex4, i.patternUV + offset), tex2D(_PatternTex5, i.patternUV + offset), 1.0- tex2D(_PatternTex3, i.uv));
                    //half4 col3 = lerp(col, col2, col2.a * _CloudsVisible);
                
                    //col3.a = col.a;
                    return col;
                }
                ENDCG
            }
        }
}