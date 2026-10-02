Shader "TCG/Battle/OriginalFieldOpaque"
{
    Properties
    {
        _Texture2D ("Original texture", 2D) = "white" {}
        _UseVertexColor ("Original vertex color", Float) = 0
        _EffectAvailable ("Effect", Float) = 0
        _Selected ("Selected", Float) = 0
        _ShadowSoftDistance ("Shadow distance", Float) = .001
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "MDPro3BattleSurface.hlsl"
            TEXTURE2D(_Texture2D); SAMPLER(sampler_Texture2D);
            CBUFFER_START(UnityPerMaterial)
            float4 _Texture2D_ST;
            float _UseVertexColor, _EffectAvailable, _Selected, _ShadowSoftDistance;
            CBUFFER_END
            half4 frag(BattleVaryings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_Texture2D, sampler_Texture2D, input.uv * _Texture2D_ST.xy + _Texture2D_ST.zw);
                color.rgb *= lerp(1, input.color.rgb, _UseVertexColor) * BattleShadow(input.positionWS);
                color.rgb += half3(.8, .75, .02) * (_Selected + _EffectAvailable * (.15 + .1 * sin(_Time.y * 4)));
                return half4(color.rgb, 1);
            }
            ENDHLSL
        }
    }
}
