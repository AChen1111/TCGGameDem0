Shader "TCG/Battle/OriginalPlayableGuide"
{
    Properties
    {
        _Texture2D ("Original guide mask", 2D) = "white" {}
        _Opacity ("Original opacity", Range(0,1)) = 1
        _Opacity02 ("Transition opacity", Float) = 0
        _Move01 ("Transition offset one", Float) = 0
        _Move02 ("Transition offset two", Float) = 0
        _Scale ("Original transition scale", Float) = 1
        _Switch ("Near color", Float) = 1
        _Swich ("Original luminous color switch", Float) = 1
        _Strength ("Original light strength", Float) = .7
        Vector1_7e240758ebcf4a538357aeb063fe9d8a ("Original luminous alpha", Range(0,1)) = 1
        _Luminous ("Luminous mesh", Float) = 0
        _ChangeLayer ("Original transition layer", Float) = 0
        _ColorP1 ("Near color", Color) = (0,.527,1,0)
        _ColorP2 ("Far color", Color) = (1,.203,.203,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex BattleVert
            #pragma fragment frag
            #include "MDPro3BattleSurface.hlsl"
            TEXTURE2D(_Texture2D); SAMPLER(sampler_Texture2D);
            CBUFFER_START(UnityPerMaterial)
            float4 _Texture2D_ST, _ColorP1, _ColorP2;
            float _Opacity,_Opacity02,_Move01,_Move02,_Scale,_Switch,_Swich,_Strength,Vector1_7e240758ebcf4a538357aeb063fe9d8a,_Luminous,_ChangeLayer;
            CBUFFER_END
            half4 frag(BattleVaryings input) : SV_Target
            {
                float2 uv = (input.uv-.5) * _Scale + .5;
                uv += float2(_Move01,_Move02);
                half4 mask = SAMPLE_TEXTURE2D(_Texture2D,sampler_Texture2D,uv * _Texture2D_ST.xy + _Texture2D_ST.zw);
                // 原 play 平面 UV.x 0..0.5 沿前边到中央；原 G 是亮边，B 是块纹理。
                half depthFade = pow(saturate(1-input.positionOS.z/25),1.65);
                half pattern = (mask.g*.55 + mask.b*.22) * depthFade * _Opacity;
                // 原 Luminous 的细分 UV 并不归一化，用原顶点位置求三边距离，避免出现硬矩形块。
                float borderDistance = min(abs(input.positionOS.z),abs(abs(input.positionOS.x)-29.9165));
                half halo = exp2(-borderDistance*borderDistance*5) * depthFade;
                half opacity = lerp(pattern,halo * Vector1_7e240758ebcf4a538357aeb063fe9d8a * _Strength,_Luminous);
                opacity *= lerp(1,_Opacity02,_ChangeLayer);
                half3 color = lerp(_ColorP2.rgb,_ColorP1.rgb,_Switch) * (1+mask.g*1.4+_Luminous*1.8);
                return half4(color, saturate(opacity));
            }
            ENDHLSL
        }
    }
}
