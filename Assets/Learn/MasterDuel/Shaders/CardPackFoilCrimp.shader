Shader "UI/MasterDuel/CardPackFoilCrimp"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Crimp Region)]
        _TopCrimpHeight ("Top Crimp Height", Range(0.0, 0.4)) = 0.08
        _BottomCrimpHeight ("Bottom Crimp Height", Range(0.0, 0.4)) = 0.08
        _CrimpTransition ("Edge Smoothness", Range(0.001, 0.05)) = 0.005

        [Header(Crimp Pattern)]
        _CrimpDensity ("Pattern Density (X, Y)", Vector) = (120, 100, 0, 0)
        _CrimpDepth ("Crimp Relief Depth", Range(0.0, 2.0)) = 0.8
        _Darkening ("Crevice Shadow Strength", Range(0.0, 1.0)) = 0.45

        [Header(Metallic Specular)]
        [HDR] _SpecularColor ("Specular Tint", Color) = (1.5, 1.35, 1.0, 1)
        _SpecularPower ("Specular Gloss", Range(2.0, 64.0)) = 18.0
        _SpecularIntensity ("Specular Intensity", Range(0.0, 3.0)) = 1.2
        _LightAngle ("Light Angle (Degrees)", Range(0, 360)) = 55.0
        _LightElevation ("Light Elevation", Range(0.1, 1.0)) = 0.65

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

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _TopCrimpHeight;
            float _BottomCrimpHeight;
            float _CrimpTransition;
            float4 _CrimpDensity;
            float _CrimpDepth;
            float _Darkening;

            fixed4 _SpecularColor;
            float _SpecularPower;
            float _SpecularIntensity;
            float _LightAngle;
            float _LightElevation;

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

            // 计算压纹高度及解析导数 (用于零开销生成表面法线)
            void EvaluateCrimp(float2 uv, out float height, out float2 dH)
            {
                float kx = _CrimpDensity.x * 6.2831853;
                float ky = _CrimpDensity.y * 6.2831853;

                float sx = sin(uv.x * kx);
                float cx = cos(uv.x * kx);
                float sy = sin(uv.y * ky);
                float cy = cos(uv.y * ky);

                // 菱形/网格点阵复合凹凸
                height = sx * cy;

                // 偏导数 dH/dx 和 dH/dy
                dH.x = kx * cx * cy;
                dH.y = -ky * sx * sy;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 texColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                texColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(texColor.a - 0.001);
                #endif

                float uvY = IN.texcoord.y;

                // 计算顶部与底部压纹权重 (带平滑过渡)
                float topMask = smoothstep(1.0 - _TopCrimpHeight - _CrimpTransition, 1.0 - _TopCrimpHeight, uvY);
                float bottomMask = smoothstep(_BottomCrimpHeight + _CrimpTransition, _BottomCrimpHeight, uvY);
                float crimpMask = saturate(topMask + bottomMask);

                if (crimpMask > 0.001)
                {
                    float height;
                    float2 dH;
                    EvaluateCrimp(IN.texcoord, height, dH);

                    // 构建法线
                    float3 normal = normalize(float3(-dH.x * _CrimpDepth * 0.01, -dH.y * _CrimpDepth * 0.01, 1.0));

                    // 光照方向计算
                    float rad = radians(_LightAngle);
                    float3 lightDir = normalize(float3(cos(rad), sin(rad), _LightElevation));
                    float3 viewDir = float3(0.0, 0.0, 1.0);
                    float3 halfDir = normalize(lightDir + viewDir);

                    // 漫反射与缝隙暗影
                    float NdotL = saturate(dot(normal, lightDir));
                    float shadow = lerp(1.0 - _Darkening, 1.0, NdotL * (height * 0.5 + 0.5));

                    // 金属高光
                    float NdotH = saturate(dot(normal, halfDir));
                    float spec = pow(NdotH, _SpecularPower) * _SpecularIntensity;

                    // 混合效果
                    float3 crimpedColor = texColor.rgb * shadow + _SpecularColor.rgb * spec;
                    texColor.rgb = lerp(texColor.rgb, crimpedColor, crimpMask);
                }

                return texColor;
            }
            ENDCG
        }
    }
}
