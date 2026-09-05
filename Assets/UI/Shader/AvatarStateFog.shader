Shader "UI/Avatar/StateFog"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Fog)]
        _FogColor ("Fog Color", Color) = (0.015, 0.015, 0.025, 1)
        _Density ("Fog Density", Range(0.0, 1.0)) = 0.82
        _NoiseStrength ("Noise Strength", Range(0.0, 1.0)) = 0.45
        _NoiseScale ("Noise Scale", Range(1.0, 16.0)) = 4.0
        _Contrast ("Noise Contrast", Range(0.5, 4.0)) = 1.4
        _ScrollSpeed ("Scroll Speed (layer1 xy, layer2 zw)", Vector) = (0.05, 0.02, -0.03, 0.035)

        [Header(Flow)]
        _Warp ("Flow Warp", Range(0.0, 1.0)) = 0.4
        _WarpScale ("Warp Scale", Range(0.2, 4.0)) = 0.8
        _WarpSpeed ("Warp Speed", Range(0.0, 1.0)) = 0.12

        [Header(Edge)]
        _EdgeDarken ("Edge Darken", Range(0.0, 1.0)) = 0.45
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.12
        _EdgeSoftness ("Edge Softness", Range(0.0005, 0.05)) = 0.006

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

            fixed4 _FogColor;
            float _Density;
            float _NoiseStrength;
            float _NoiseScale;
            float _Contrast;
            float4 _ScrollSpeed;

            float _Warp;
            float _WarpScale;
            float _WarpSpeed;

            float _EdgeDarken;
            float _CornerRadius;
            float _EdgeSoftness;

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

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // 双线性插值的 value noise, 比 tex 采样省一张图
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

            float RoundedBoxSDF(float2 p, float2 halfSize, float radius)
            {
                float2 q = abs(p) - halfSize + radius;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float tm = _Time.y;
                float2 uv = IN.texcoord * _NoiseScale;

                // 先用一层低频噪声扰动采样坐标, 雾团才会翻涌而不是整体平移
                float2 warp = float2(
                    ValueNoise(uv * _WarpScale + float2(0.0, tm * _WarpSpeed)),
                    ValueNoise(uv * _WarpScale + float2(31.4, -tm * _WarpSpeed * 0.8))) * 2.0 - 1.0;
                uv += warp * _Warp * _NoiseScale * 0.25;

                // 两层反向流动的噪声叠加, 形成缓慢翻涌的雾团
                float n1 = Fbm(uv + _ScrollSpeed.xy * tm);
                float n2 = Fbm(uv * 2.13 + _ScrollSpeed.zw * tm);
                float n = pow(saturate(n1 * 0.65 + n2 * 0.35), _Contrast);

                float fogMask = lerp(1.0 - _NoiseStrength, 1.0, n);

                // 越靠边缘越浓, 中心留一点透光让头像轮廓隐约可见
                float2 d = abs(IN.texcoord - 0.5) * 2.0;
                float vignette = smoothstep(0.2, 1.0, max(d.x, d.y));
                float alpha = saturate(_Density * fogMask + vignette * _EdgeDarken);

                // 圆角裁掉四角, 和光环形状保持一致
                float2 halfSize = float2(0.5, 0.5);
                float radius = min(_CornerRadius, 0.5);
                float sd = RoundedBoxSDF(IN.texcoord - 0.5, halfSize, radius);
                alpha *= 1.0 - smoothstep(-_EdgeSoftness, _EdgeSoftness, sd);

                fixed4 outColor = fixed4(_FogColor.rgb * IN.color.rgb, alpha * _FogColor.a * IN.color.a);

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
