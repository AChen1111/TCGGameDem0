Shader "TCG/Battle/OriginalTimerBody"
{
    Properties
    {
        _Texture2D ("Original texture", 2D) = "white" {}
        _SwitchTurn ("Acting player", Range(0,1)) = 0
        _Active ("Active", Range(0,1)) = 1
        _MouseOver ("Hover", Float) = 0
        _PressButton ("Pressed", Float) = 0
        _ColorMouseOverNear ("Near hover", Color) = (0,.675,1,1)
        _ColorPlayNear ("Near active", Color) = (0,.36,1,0)
        _ColorMouseOverFar ("Far hover", Color) = (.9,.02,.02,1)
        _ColorPlayFar ("Far active", Color) = (.114,.039,.039,0)
        _ColorP1 ("Near light", Color) = (.175,.743,1,0)
        _ColorP2 ("Far light", Color) = (1,.08,.173,0)
        _InactiveIntensityNear ("Near paused", Float) = .5
        _InactiveIntensityFar ("Far paused", Float) = .5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off ZWrite On
            HLSLPROGRAM
            #pragma vertex BattleVert
            #pragma fragment frag
            #include "MDPro3BattleSurface.hlsl"
            TEXTURE2D(_Texture2D); SAMPLER(sampler_Texture2D);
            CBUFFER_START(UnityPerMaterial)
            float4 _Texture2D_ST, _ColorMouseOverNear, _ColorPlayNear, _ColorMouseOverFar, _ColorPlayFar, _ColorP1, _ColorP2;
            float _SwitchTurn, _Active, _MouseOver, _PressButton, _InactiveIntensityNear, _InactiveIntensityFar;
            CBUFFER_END
            half4 frag(BattleVaryings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_Texture2D, sampler_Texture2D, input.uv * _Texture2D_ST.xy + _Texture2D_ST.zw);
                half blueMask = saturate((color.b - max(color.r,color.g)) * 2);
                half redMask = saturate((color.r-max(color.g,color.b)) * 2) * (1-step(.3,color.g));
                half lightMask = max(blueMask,redMask);
                half paused = lerp(_InactiveIntensityNear,_InactiveIntensityFar,_SwitchTurn);
                // 保留原蓝玻璃贴图的 G 通道明暗和高光，不能用几乎恒定的 B 通道替换整个玻璃面。
                half3 nearGlass = color.rgb * half3(1,_ColorP1.g/.74285,_ColorP1.b);
                half3 farGlass = half3(color.b, color.g*_ColorP2.g, color.r + color.g*_ColorP2.b);
                color.rgb = lerp(color.rgb, lerp(nearGlass,farGlass,_SwitchTurn),lightMask);
                color.rgb *= lerp(1,lerp(paused,1,_Active),lightMask);
                color.rgb += lerp(_ColorMouseOverNear.rgb,_ColorMouseOverFar.rgb,_SwitchTurn) * lightMask * (_MouseOver*.25 + _PressButton*.4);
                return half4(color.rgb,1);
            }
            ENDHLSL
        }
    }
}
