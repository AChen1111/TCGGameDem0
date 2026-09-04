Shader "UI/Avatar/RingFlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Ring Shape)]
        _Inset ("Ring Inset", Range(0.0, 0.3)) = 0.03
        _Thickness ("Ring Thickness", Range(0.001, 0.2)) = 0.035
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.12
        _EdgeSoftness ("Edge Softness", Range(0.0005, 0.05)) = 0.006

        [Header(Base Glow)]
        _BaseAlpha ("Base Alpha", Range(0.0, 1.0)) = 0.55
        _OuterGlow ("Outer Glow Width", Range(0.0, 0.2)) = 0.05
        _OuterGlowAlpha ("Outer Glow Alpha", Range(0.0, 1.0)) = 0.3

        [Header(Edge Flow)]
        _FlowSpeed ("Flow Speed (laps per second)", Range(-2.0, 2.0)) = 0.35
        _FlowScale ("Flow Detail", Range(1.0, 12.0)) = 5.5
        _FlowEvolve ("Flow Evolve Speed", Range(0.0, 1.0)) = 0.15
        _Irregularity ("Thickness Irregularity", Range(0.0, 1.0)) = 0.7
        _Flicker ("Brightness Flicker", Range(0.0, 1.0)) = 0.5
        _Glow ("Glow Boost", Range(0.0, 3.0)) = 0.9
        [HDR] _FlowColor ("Flow Highlight Tint", Color) = (1, 1, 1, 1)

        [Header(UI Stencil Masking)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Inset;
            float _Thickness;
            float _CornerRadius;
            float _EdgeSoftness;

            float _BaseAlpha;
            float _OuterGlow;
            float _OuterGlowAlpha;

            float _FlowSpeed;
            float _FlowScale;
            float _FlowEvolve;
            float _Irregularity;
            float _Flicker;
            float _Glow;
            fixed4 _FlowColor;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // 圆角矩形有符号距离: 负值在内部, 正值在外部, 0 即边界
            float RoundedBoxSDF(float2 p, float2 halfSize, float radius)
            {
                float2 q = abs(p) - halfSize + radius;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash(i);
                float b = Hash(i + float2(1.0, 0.0));
                float c = Hash(i + float2(0.0, 1.0));
                float d = Hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float Fbm(float2 p)
            {
                float v = ValueNoise(p) * 0.6;
                v += ValueNoise(p * 2.17 + 13.7) * 0.3;
                v += ValueNoise(p * 4.31 + 41.3) * 0.1;
                return v;
            }

            // 沿圆周采样噪声, 首尾天然无缝; 角度随时间推进即为图案绕边缘流动
            float EdgeFlowNoise(float2 p)
            {
                float t = atan2(p.y, p.x) * 0.15915494;

                float a1 = (t + _Time.y * _FlowSpeed) * 6.2831853;
                float n1 = Fbm(float2(cos(a1), sin(a1)) * _FlowScale + _Time.y * _FlowEvolve);

                // 第二层反向且更慢, 叠加后看不出循环周期
                float a2 = (t - _Time.y * _FlowSpeed * 0.55) * 6.2831853;
                float n2 = Fbm(float2(cos(a2), sin(a2)) * (_FlowScale * 1.9) - _Time.y * _FlowEvolve * 0.7);

                return saturate(n1 * 0.65 + n2 * 0.35);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 p = IN.texcoord - 0.5;
                float2 halfSize = max(0.5 - _Inset, 0.001);
                float radius = min(_CornerRadius, min(halfSize.x, halfSize.y));
                float sd = RoundedBoxSDF(p, halfSize, radius);

                float n = EdgeFlowNoise(p);

                // 环带厚度随噪声起伏, 边缘因此呈不规则涌动而非匀宽描边
                float halfT = max(_Thickness * 0.5 * (1.0 + (n * 2.0 - 1.0) * _Irregularity), 0.0005);
                float band = 1.0 - smoothstep(halfT - _EdgeSoftness, halfT + _EdgeSoftness, abs(sd));

                // 火舌只向环带外侧扩散, 长度由噪声决定; step 必须保留, 否则环带内侧
                // 与头像内部的 outward 都被夹成 0, 会被降序 smoothstep 整片填成实色.
                float outward = sd - halfT;
                float flareWidth = max(_OuterGlow * (0.25 + n * 0.75), 1e-5);
                float flare = smoothstep(flareWidth, 0.0, outward) * _OuterGlowAlpha * step(0.0, outward);

                float pulse = lerp(1.0 - _Flicker, 1.0 + _Flicker, n);
                float alpha = saturate((band * _BaseAlpha + flare) * pulse);

                // 亮度主要走 alpha, 颜色只轻微提亮; 直接乘 n 会让高饱和色截断后褪成白色
                float3 rgb = IN.color.rgb * (1.0 + n * _Glow * 0.35)
                    + _FlowColor.rgb * pow(n, 4.0) * _Glow * 0.25;

                fixed4 outColor = fixed4(rgb, alpha * IN.color.a);

                #ifdef UNITY_UI_CLIP_RECT
                outColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(outColor.a - 0.001);
                #endif

                return outColor;
            }
            ENDCG
        }
    }
}
