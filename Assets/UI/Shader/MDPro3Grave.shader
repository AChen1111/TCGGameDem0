Shader "TCG/Battle/OriginalGrave"
{
    Properties
    {
        _Texture2D ("Original base", 2D) = "white" {}
        _Texture2DLight ("Original channel mask", 2D) = "black" {}
        _GraveCardExist ("Grave occupied", Float) = 0
        _GraveMouseOver ("Grave hover", Float) = 0
        _GravePressButton ("Grave pressed", Float) = 0
        _ExcludeCardExist ("Exclude occupied", Float) = 0
        _ExcludeMouseOver ("Exclude hover", Float) = 0
        _ExcludePressButton ("Exclude pressed", Float) = 0
        _GraveEffectAvailable ("Grave effect", Float) = 0
        _ExcludeEffectAvailable ("Exclude effect", Float) = 0
        _IntensityGraveCardExist ("Grave idle intensity", Float) = .4
        _IntensityGraveMouseOver ("Grave hover intensity", Float) = .1
        _IntensityExcludeCardExist ("Exclude idle intensity", Float) = .4
        _IntensityExcludeMouseOver ("Exclude hover intensity", Float) = .1
        _BaseColorExclude ("Exclude light", Color) = (.885,0,1,0)
        _BaseColorGrave ("Grave light", Color) = (0,.158,.953,0)
        _HighlightExclude ("Exclude highlight", Color) = (.949,.288,1,0)
        _HighlightGrave ("Grave highlight", Color) = (0,.441,1,0)
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
            #include "MDPro3BattleSurface.hlsl"
            TEXTURE2D(_Texture2D); SAMPLER(sampler_Texture2D);
            TEXTURE2D(_Texture2DLight); SAMPLER(sampler_Texture2DLight);
            CBUFFER_START(UnityPerMaterial)
            float4 _Texture2D_ST, _Texture2DLight_ST, _BaseColorExclude, _BaseColorGrave, _HighlightExclude, _HighlightGrave;
            float _GraveCardExist, _GraveMouseOver, _GravePressButton, _ExcludeCardExist, _ExcludeMouseOver, _ExcludePressButton;
            float _GraveEffectAvailable, _ExcludeEffectAvailable;
            float _IntensityGraveCardExist, _IntensityGraveMouseOver, _IntensityExcludeCardExist, _IntensityExcludeMouseOver;
            CBUFFER_END
            half4 frag(BattleVaryings input) : SV_Target
            {
                half3 color = SAMPLE_TEXTURE2D(_Texture2D, sampler_Texture2D, input.uv * _Texture2D_ST.xy + _Texture2D_ST.zw).rgb;
                half2 masks = SAMPLE_TEXTURE2D(_Texture2DLight, sampler_Texture2DLight, input.uv * _Texture2DLight_ST.xy + _Texture2DLight_ST.zw).rg;
                half pulse = .65 + .35 * sin(_Time.y * 4);
                half grave = _GraveCardExist * _IntensityGraveCardExist + _GraveMouseOver * _IntensityGraveMouseOver;
                half exclude = _ExcludeCardExist * _IntensityExcludeCardExist + _ExcludeMouseOver * _IntensityExcludeMouseOver;
                // 原 Light 贴图：绿色通道是墓地，红色通道是除外。
                color += masks.g * (_BaseColorGrave.rgb * grave + _HighlightGrave.rgb * (_GravePressButton + _GraveEffectAvailable * pulse));
                color += masks.r * (_BaseColorExclude.rgb * exclude + _HighlightExclude.rgb * (_ExcludePressButton + _ExcludeEffectAvailable * pulse));
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
