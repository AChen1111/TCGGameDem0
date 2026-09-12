Shader "Card/HoloFoil"
{
    Properties
    {
        [MainTexture] _BaseMap ("Card Art", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _HoloStrength ("Holo Strength", Range(0, 1)) = 0.55
        _HoloScale ("Streak Density", Range(0.5, 12)) = 5
        _HoloSpeed ("Flow Speed", Range(0, 2)) = 0.25
        _Sparkle ("Sparkle", Range(0, 1)) = 0.4
        [HDR] _HoloTint ("Holo Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
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

            // 余弦调色盘, t 滑动时走一圈彩虹
            half3 HoloPalette(half t)
            {
                return 0.5 + 0.5 * cos(6.2831853 * (t + half3(0.00, 0.33, 0.67)));
            }

            half Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                // 物体空间视线: 倾斜或翻面时彩虹条跟着走
                float3 viewOS = normalize(TransformWorldToObject(_WorldSpaceCameraPos) - input.positionOS);
                float shift = viewOS.x * 0.85 + viewOS.y * 0.35;
                float t = input.uv.x * _HoloScale + input.uv.y * (_HoloScale * 0.35) + shift + _Time.y * _HoloSpeed;
                half3 rainbow = HoloPalette(t) * _HoloTint.rgb;

                float stripe = saturate(0.35 + 0.65 * sin((input.uv.x * 2.2 + input.uv.y * 0.8 + shift) * 6.2831853));
                float spark = pow(Hash21(input.uv * 160.0 + floor(_Time.y * 8.0)), 12.0) * _Sparkle;
                half mask = saturate(stripe + spark);

                albedo.rgb = lerp(albedo.rgb, albedo.rgb * 0.65 + rainbow * 0.9, _HoloStrength * mask);
                return albedo;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
