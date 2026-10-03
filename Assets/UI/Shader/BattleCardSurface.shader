Shader "TCG/Battle/CardSurface"
{
    Properties
    {
        _BaseMap("Card / pile texture", 2D) = "white" {}
        _CardUvRect("Card atlas UV", Vector) = (0,0,1,1)
        _Tint("Tint", Color) = (1,1,1,1)
        _EffectAvailable("Effect available", Float) = 0
        _Negated("Effect negated", Float) = 0
        _Selected("Selected", Float) = 0
        _Hint("Placement target", Float) = 0
        _GlowColor("Effect glow", Color) = (1,0.72,0.12,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        Pass
        {
            Tags { "LightMode"="UniversalForwardOnly" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST, _Tint, _GlowColor;
            CBUFFER_END
            UNITY_INSTANCING_BUFFER_START(CardProperties)
                UNITY_DEFINE_INSTANCED_PROP(float4, _CardUvRect)
                UNITY_DEFINE_INSTANCED_PROP(float, _EffectAvailable)
                UNITY_DEFINE_INSTANCED_PROP(float, _Selected)
                UNITY_DEFINE_INSTANCED_PROP(float, _Hint)
                UNITY_DEFINE_INSTANCED_PROP(float, _Negated)
            UNITY_INSTANCING_BUFFER_END(CardProperties)
            Varyings Vert(Attributes v)
            { Varyings o; UNITY_SETUP_INSTANCE_ID(v); UNITY_TRANSFER_INSTANCE_ID(v, o); o.positionCS = TransformObjectToHClip(v.positionOS.xyz); o.uv = v.uv; return o; }
            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float4 rect = UNITY_ACCESS_INSTANCED_PROP(CardProperties, _CardUvRect);
                float available = UNITY_ACCESS_INSTANCED_PROP(CardProperties, _EffectAvailable);
                float selected = UNITY_ACCESS_INSTANCED_PROP(CardProperties, _Selected);
                float hint = UNITY_ACCESS_INSTANCED_PROP(CardProperties, _Hint);
                float negated = UNITY_ACCESS_INSTANCED_PROP(CardProperties, _Negated);
                float2 atlasUV = i.uv * rect.zw + rect.xy;
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, atlasUV) * _Tint;
                float edge = min(min(i.uv.x, 1-i.uv.x), min(i.uv.y, 1-i.uv.y));
                if (hint > 0.5)
                {
                    float frame = 1 - smoothstep(0.012, 0.024, edge);
                    clip(frame - 0.05);
                    return half4(float3(1,0.87,0.05) * (1.1+0.2*sin(_Time.y*4)),1);
                }
                clip(c.a - 0.04);
                c.rgb = lerp(c.rgb, dot(c.rgb, float3(0.2126,0.7152,0.0722)).xxx, negated);
                float border = 1 - smoothstep(0.025, 0.065, edge);
                float pulse = 0.65 + 0.35 * sin(_Time.y * 4);
                c.rgb += _GlowColor.rgb * border * available * pulse * 1.8;
                c.rgb += float3(0.34,0.82,0.9) * border * selected;
                c.rgb = lerp(c.rgb, float3(0.45,0.85,0.55), hint * (0.25 + border * 0.6));
                return c;
            }
            ENDHLSL
        }
    }
}
