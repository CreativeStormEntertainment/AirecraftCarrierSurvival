//
//  Based On Chris Nol shader
//

Shader "Custom/Outline Fill" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest2", Float) = 1
    _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
    _OutlineWidth("Outline Width", Range(0, 10)) = 2
    _OutlinePingPong("Hover", Range(0, 2)) = 1
  }

  SubShader {
    Tags {
      "Queue" = "AlphaTest+110"
      "RenderType" = "Transparent"
      "DisableBatching" = "True"
    }

    Pass {
      Name "Fill"
      Cull Off
      ZTest [_ZTest2]
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask RGB

      Stencil {
        Ref 1
        Comp NotEqual
      }

      CGPROGRAM
      #include "UnityCG.cginc"

      #pragma vertex vert
      #pragma fragment frag

      struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 position : SV_POSITION;
        fixed4 color : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      uniform fixed4 _OutlineColor;
      uniform float _OutlineWidth;
      uniform float _OutlinePingPong;


      v2f vert(appdata input) {
        v2f output;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        float3 normal = any(input.smoothNormal) ? input.smoothNormal : input.normal;
        float3 viewPosition = UnityObjectToViewPos(input.vertex);
        float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));
        float ab = _OutlineWidth;
        ab *= 1.0f - ((abs(fmod(_Time.w * 1.0, 2.0) - 1.0)) * _OutlinePingPong);
        output.position = UnityViewToClipPos(viewPosition + viewNormal * -viewPosition.z * ab / 1000.0);
        output.color = _OutlineColor;

        return output;
      }
 
      fixed4 frag(v2f input) : SV_Target{
      float a = 1.0f;
      a = 1.0f;// -((abs(fmod(_Time.w * 1.0, 2.0) - 1.0)) * _OutlinePingPong);
      return float4(input.color.rgb,a);
      }
      ENDCG
    }
  }
}
