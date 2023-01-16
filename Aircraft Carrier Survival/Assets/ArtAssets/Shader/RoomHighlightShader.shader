Shader "Custom/RoomHighlightShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,0.5)
        _Min("Min", float) = 0
        _Max("Max", float) = 1
        _TimeScale("TimeScale", float) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200


        CGPROGRAM
        
        
        
        #pragma surface surf Standard fullforwardshadows alpha

        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
        float _Min;
        float _Max;
        float _TimeScale;
        float _UnscaledTime;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha = lerp(_Min, _Max, sin(_UnscaledTime * _TimeScale) + 1.0 / 2.0);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
