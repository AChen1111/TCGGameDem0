Shader "Card/CardEffect"
{
    Properties
    {
        [MainTexture] _BaseMap ("Card Art", 2D) = "white" {}
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
        [KeywordEnum(None, Colorful, Mirror, Outline)] _Effect ("Effect", Float) = 1

        [Header(Colorful)]
        _HoloStrength ("Holo Strength", Range(0, 1)) = 0.55
        _HoloScale ("Streak Density", Range(0.5, 12)) = 5
        _HoloSpeed ("Flow Speed", Range(0, 2)) = 0.25
        _Sparkle ("Sparkle", Range(0, 1)) = 0.4
        [HDR] _HoloTint ("Holo Tint", Color) = (1, 1, 1, 1)

        [Header(Mirror)]
        _MirrorScale ("Mirror Scale", Range(2, 20)) = 8
        _MirrorStrength ("Mirror Strength", Range(0, 1)) = 0.55

        [Header(Outline)]
        _OutlineWidth ("Outline Width", Range(0.005, 0.12)) = 0.035
        [HDR] _OutlineColor ("Outline Color", Color) = (1, 0.86, 0.35, 1)

        [Header(Dissolve)]
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _DissolveScale ("Dissolve Noise Scale", Range(1, 24)) = 10
        _DissolveDir ("Dissolve Direction", Range(0, 1)) = 0.55
        _DissolveEdgeWidth ("Dissolve Edge", Range(0.01, 0.25)) = 0.08
        [HDR] _DissolveEdgeColor ("Dissolve Edge Color", Color) = (2.2, 0.75, 0.18, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _EFFECT_NONE _EFFECT_COLORFUL _EFFECT_MIRROR _EFFECT_OUTLINE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _HoloStrength;
                half _HoloScale;
                half _HoloSpeed;
                half _Sparkle;
                half4 _HoloTint;
                half _MirrorScale;
                half _MirrorStrength;
                half _OutlineWidth;
                half4 _OutlineColor;
                half _Dissolve;
                half _DissolveScale;
                half _DissolveDir;
                half _DissolveEdgeWidth;
                half4 _DissolveEdgeColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half3 HoloPalette(half t)
            {
                return 0.5 + 0.5 * cos(6.2831853 * (t + half3(0.00, 0.33, 0.67)));
            }

            half Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            half ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                half a = Hash21(i);
                half b = Hash21(i + float2(1.0, 0.0));
                half c = Hash21(i + float2(0.0, 1.0));
                half d = Hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            half4 SampleArt(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
            }

            half4 ApplyColorful(half4 albedo, float2 uv, float3 positionOS)
            {
                float3 viewOS = normalize(TransformWorldToObject(_WorldSpaceCameraPos) - positionOS);
                float shift = viewOS.x * 0.85 + viewOS.y * 0.35;
                float t = uv.x * _HoloScale + uv.y * (_HoloScale * 0.35) + shift + _Time.y * _HoloSpeed;
                half3 rainbow = HoloPalette(t) * _HoloTint.rgb;
                float stripe = saturate(0.35 + 0.65 * sin((uv.x * 2.2 + uv.y * 0.8 + shift) * 6.2831853));
                float spark = pow(Hash21(uv * 160.0 + floor(_Time.y * 8.0)), 12.0) * _Sparkle;
                half mask = saturate(stripe + spark);
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * 0.65 + rainbow * 0.9, _HoloStrength * mask);
                return albedo;
            }

            half4 ApplyMirror(half4 albedo, float2 uv)
            {
                float2 grid = floor(uv * _MirrorScale);
                float n = Hash21(grid);
                float2 offset = float2(n, Hash21(grid + 17.13)) * 2.0 - 1.0;
                float2 shardUv = uv + offset * (_MirrorStrength * 0.045);
                albedo = SampleArt(shardUv);
                float2 f = frac(uv * _MirrorScale) - 0.5;
                float crack = pow(1.0 - saturate(min(abs(f.x), abs(f.y)) * 16.0), 10.0);
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * 1.25 + 0.12, crack * _MirrorStrength);
                return albedo;
            }

            half4 ApplyOutline(half4 albedo, float2 uv)
            {
                float e = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float outline = 1.0 - smoothstep(0.0, _OutlineWidth, e);
                albedo.rgb = lerp(albedo.rgb, _OutlineColor.rgb, outline);
                return albedo;
            }

            // 噪声 clip + 边缘自发光. Dir 越大越从下往上烧
            half4 ApplyDissolve(half4 albedo, float2 uv)
            {
                half noise = ValueNoise(uv * _DissolveScale);
                half mask = lerp(noise, saturate(uv.y * 0.65 + noise * 0.35), _DissolveDir);
                half cutoff = lerp(-0.001, 1.001, _Dissolve);
                clip(mask - cutoff);

                half edge = 1.0 - saturate((mask - cutoff) / max(_DissolveEdgeWidth, 1e-4));
                edge *= step(0.001, _Dissolve);
                albedo.rgb += _DissolveEdgeColor.rgb * edge;
                return albedo;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = SampleArt(input.uv);

                #if defined(_EFFECT_MIRROR)
                    albedo = ApplyMirror(albedo, input.uv);
                #elif defined(_EFFECT_OUTLINE)
                    albedo = ApplyOutline(albedo, input.uv);
                #elif defined(_EFFECT_COLORFUL)
                    albedo = ApplyColorful(albedo, input.uv, input.positionOS);
                #endif

                // 用物体空间 XY, 正面/背面/夹层共用同一套溶解遮罩
                return ApplyDissolve(albedo, input.positionOS.xy + 0.5);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
