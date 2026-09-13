Shader "Card/CardEffect"
{
    Properties
    {
        [MainTexture] _BaseMap ("Card Art", 2D) = "white" {}
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
        [KeywordEnum(None, Colorful, Mirror, Outline, Gold)] _Effect ("Effect", Float) = 1

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
        _OutlineFilm ("Gold Film", Range(0, 1)) = 1
        _OutlineSweepSpeed ("Sweep Speed", Range(0, 2)) = 0.2
        _OutlineSweepWidth ("Sweep Width", Range(0.02, 0.35)) = 0.05
        _OutlineSweepStrength ("Sweep Strength", Range(0, 2)) = 0.85

        [Header(Gold)]
        [HDR] _GoldColor ("Gold Color", Color) = (1.45, 1.02, 0.32, 1)
        _GoldNoiseMap ("Gold Noise", 2D) = "gray" {}
        _GoldWidth ("Gold Width", Range(0.006, 0.08)) = 0.028
        _GoldFlowSpeed ("Gold Flow Speed", Range(0, 4)) = 0.85
        _GoldSandDensity ("Gold Sand Density", Range(2, 20)) = 8
        _GoldSparkle ("Gold Sparkle", Range(0, 2)) = 0.95
        _GoldOutline ("Gold Outline", Range(0, 1)) = 1

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
            #pragma multi_compile_local _EFFECT_NONE _EFFECT_COLORFUL _EFFECT_MIRROR _EFFECT_OUTLINE _EFFECT_GOLD
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_GoldNoiseMap);
            SAMPLER(sampler_GoldNoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _GoldNoiseMap_ST;
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
                half _OutlineFilm;
                half _OutlineSweepSpeed;
                half _OutlineSweepWidth;
                half _OutlineSweepStrength;
                half4 _GoldColor;
                half _GoldWidth;
                half _GoldFlowSpeed;
                half _GoldSandDensity;
                half _GoldSparkle;
                half _GoldOutline;
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

            half4 SampleGoldNoise(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_GoldNoiseMap, sampler_GoldNoiseMap, TRANSFORM_TEX(uv, _GoldNoiseMap));
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

            half4 ApplyOutline(half4 albedo, float2 uv, float3 positionOS)
            {
                float e = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float outline = 1.0 - smoothstep(0.0, _OutlineWidth, e);
                albedo.rgb = lerp(albedo.rgb, _OutlineColor.rgb, outline);

                // 整卡金色薄膜, 噪声让膜面不平, 边缘随视角略亮
                float3 viewOS = normalize(TransformWorldToObject(_WorldSpaceCameraPos) - positionOS);
                half4 nFilm = SampleGoldNoise(uv);
                float fresnel = pow(1.0 - saturate(abs(viewOS.z)), 2.2);
                float film = _OutlineFilm * (0.5 + fresnel * 0.7) * (0.82 + nFilm.a * 0.36);
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * _OutlineColor.rgb, film);

                // 斜向扫光, 从左下往右上周期性扫过
                float sweepPos = frac(_Time.y * _OutlineSweepSpeed);
                float axis = uv.x * 0.85 + uv.y * 0.45;
                float sweep = 1.0 - saturate(abs(axis - lerp(-0.18, 1.28, sweepPos)) / max(_OutlineSweepWidth, 1e-4));
                sweep = pow(sweep, 2.8);
                albedo.rgb += _OutlineColor.rgb * sweep * _OutlineSweepStrength;
                return albedo;
            }

            // 单位 UV 矩形周长, 底边左端起顺时针
            float GoldPerimeter(float2 uv)
            {
                float left = uv.x;
                float right = 1.0 - uv.x;
                float bottom = uv.y;
                float top = 1.0 - uv.y;
                float nearest = min(min(left, right), min(bottom, top));
                if (left <= nearest + 1e-5)
                {
                    return 3.0 + (1.0 - uv.y);
                }

                if (right <= nearest + 1e-5)
                {
                    return 1.0 + uv.y;
                }

                if (bottom <= nearest + 1e-5)
                {
                    return uv.x;
                }

                return 2.0 + (1.0 - uv.x);
            }

            half4 ApplyGold(half4 albedo, float2 uv, float3 positionOS)
            {
                // 卡面金沙: 噪声筛出沙粒后按时相闪, 视角只做加成
                float3 viewOS = normalize(TransformWorldToObject(_WorldSpaceCameraPos) - positionOS);
                half4 nFace = SampleGoldNoise(uv);
                half4 nSpark = SampleGoldNoise(uv * 3.2 + nFace.rg * 0.03);
                float seed = frac(nSpark.r * 17.27 + nSpark.g * 9.13 + nSpark.b * 5.41);
                float grain = saturate((seed - 0.87) * 12.0);
                grain *= grain;
                float twinkle = pow(saturate(sin(_Time.y * (1.15 + nSpark.b * 1.25) + nSpark.r * 28.0) * 0.5 + 0.5), 3.0);
                float3 flakeN = nSpark.rgb * 2.0 - 1.0;
                flakeN = normalize(float3(flakeN.xy, max(flakeN.z, 0.2)));
                float3 lightOS = normalize(float3(0.2, 0.55, 1.0) + viewOS * 0.2);
                float spec = pow(saturate(dot(reflect(-lightOS, flakeN), viewOS)), 8.0);
                float viewSweep = 1.0 - abs(frac(uv.x * 0.8 + viewOS.x * 0.7 + nFace.g * 0.15) * 2.0 - 1.0);
                viewSweep = pow(saturate(viewSweep), 2.2);
                float glitter = grain * (twinkle * 1.25 + spec * 0.55) * _GoldSparkle;
                albedo.rgb = lerp(albedo.rgb, _GoldColor.rgb * 1.55, saturate(glitter));
                albedo.rgb += _GoldColor.rgb * glitter * 1.1;

                if (_GoldOutline <= 0.5)
                {
                    return albedo;
                }

                float e = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float band = 1.0 - smoothstep(0.0, _GoldWidth, e);
                float peri = GoldPerimeter(uv);
                float2 warp = nFace.rg - 0.5;
                float2 sandUv = float2(
                    peri * _GoldSandDensity * 0.22 - _Time.y * _GoldFlowSpeed * 0.08,
                    e * (_GoldSandDensity * 0.85)) + warp * 0.12;
                half4 nEdge = SampleGoldNoise(sandUv);
                float metal = saturate(nEdge.b * 0.55 + nEdge.g * 0.45);
                float edgeGrain = pow(nEdge.a, 3.2);
                float flakeGrain = edgeGrain * step(0.58, nEdge.a);

                half3 dark = _GoldColor.rgb * 0.45;
                half3 mid = _GoldColor.rgb;
                half3 hot = _GoldColor.rgb * half3(1.8, 1.4, 0.7) + 0.25;
                half3 gold = lerp(dark, mid, saturate(metal * 1.25 + 0.25));
                gold = lerp(gold, hot, flakeGrain * (0.55 + viewSweep));

                albedo.rgb = lerp(albedo.rgb, gold, saturate(band * (0.9 + metal * 0.2)));
                albedo.rgb += hot * (0.28 + flakeGrain) * band * 1.6;
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
                    albedo = ApplyOutline(albedo, input.uv, input.positionOS);
                #elif defined(_EFFECT_COLORFUL)
                    albedo = ApplyColorful(albedo, input.uv, input.positionOS);
                #elif defined(_EFFECT_GOLD)
                    albedo = ApplyGold(albedo, input.uv, input.positionOS);
                #endif

                // 用物体空间 XY, 正面/背面/夹层共用同一套溶解遮罩
                return ApplyDissolve(albedo, input.positionOS.xy + 0.5);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
