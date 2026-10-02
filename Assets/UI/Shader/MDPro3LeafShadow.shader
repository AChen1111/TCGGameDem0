Shader "TCG/Battle/OriginalLeafShadow"
{
    Properties
    {
        _Texture2D ("Original mask", 2D) = "white" {}
        _ShadowColor ("Original shadow color", Color) = (0,0,0,0)
        _ShadowIntensity ("Original intensity", Range(-1,0)) = -.5
        _AddAlpha ("Original opacity", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-10" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Offset -1,-1
            HLSLPROGRAM
            #pragma vertex BattleVert
            #pragma fragment frag
            #include "MDPro3BattleSurface.hlsl"
            TEXTURE2D(_Texture2D); SAMPLER(sampler_Texture2D);
            CBUFFER_START(UnityPerMaterial)
            float4 _Texture2D_ST, _ShadowColor;
            float _ShadowIntensity, _AddAlpha;
            CBUFFER_END
            half4 frag(BattleVaryings input) : SV_Target
            {
                half3 packed = SAMPLE_TEXTURE2D(_Texture2D, sampler_Texture2D, input.uv * _Texture2D_ST.xy + _Texture2D_ST.zw).rgb;
                half mask = 1 - dot(packed, half3(.299,.587,.114));
                return half4(_ShadowColor.rgb, saturate(mask * (-_ShadowIntensity + _AddAlpha)));
            }
            ENDHLSL
        }
    }
}
