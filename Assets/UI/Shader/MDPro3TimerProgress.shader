Shader "TCG/Battle/OriginalTimerProgress"
{
    Properties
    {
        _MaxTime ("Remaining fraction", Range(0,1)) = 1
        _AddTime ("Added fraction", Range(0,1)) = 0
        _Active ("Running", Float) = 1
        _SwitchTurn ("Acting player", Float) = 0
        _ColorP1 ("Near light", Color) = (.175,.743,1,0)
        _ColorP2 ("Far light", Color) = (1,.08,.173,0)
        _MaxTimeColor01 ("Original progress outer", Color) = (.212,.642,1,0)
        _MaxTimeColor02 ("Original progress inner", Color) = (0,.41,1,0)
        _AddTimeColorBar01 ("Added outer", Color) = (1,.8,0,0)
        _AddTimeColor02 ("Added inner", Color) = (1,.8,0,0)
        _SwitchAddToMax ("Original progress switch", Float) = 1
        _BarBaseColor ("Empty ring", Color) = (.196,.196,.196,0)
        _BarTickness ("Original width", Range(0,1)) = .8
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
            CBUFFER_START(UnityPerMaterial)
            float4 _ColorP1, _ColorP2, _MaxTimeColor01, _MaxTimeColor02, _AddTimeColorBar01, _AddTimeColor02, _BarBaseColor;
            float _MaxTime, _AddTime, _Active, _SwitchTurn, _SwitchAddToMax, _BarTickness;
            CBUFFER_END
            half4 frag(BattleVaryings input) : SV_Target
            {
                float2 center = input.uv * 2 - 1;
                float radius = length(center);
                float angle = frac(atan2(center.x,center.y) / TWO_PI + 1);
                half filled = step(angle,_MaxTime) * step(.0001,_MaxTime);
                half3 live = lerp(_ColorP1.rgb,_ColorP2.rgb,_SwitchTurn) * lerp(.55,1,_Active);
                // 原 Gauge 子网格是完整圆盘；_BarTickness=.8 定义进度环的内半径。
                half ring = smoothstep(_BarTickness-.008,_BarTickness+.008,radius);
                half3 blackGlass = _BarBaseColor.rgb * (.035 + .055 * saturate(1-radius));
                half3 bar = lerp(_BarBaseColor.rgb*.45,live,filled);
                return half4(lerp(blackGlass,bar,ring),1);
            }
            ENDHLSL
        }
    }
}
