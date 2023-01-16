//
//  Based On Chris Nol shader
//

Shader "Custom/Outline Mask" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
     [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest2("ZTest", Float) = 0
  }

  SubShader {
    Tags {
      "Queue" = "AlphaTest+100"
      "RenderType" = "Transparent"
    }

    Pass {
      Name "Mask"
      Cull Off
      ZTest [_ZTest2]
      ZWrite Off
      ColorMask 0

      Stencil {
        Ref 1
        Pass Replace
      }
    }
  }
}
