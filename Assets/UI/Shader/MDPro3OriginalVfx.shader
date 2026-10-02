Shader "TCG/Battle/OriginalVfx"
{
    Properties
    {
        _SourceTexture ("Original texture binding", 2D) = "white" {}
        _SourceTint ("Original color binding", Color) = (1,1,1,1)
        _SourceHasTexture ("Original texture presence", Float) = 0
        _Family ("Original effect family", Float) = 0
        _SrcBlend ("Original source blend", Float) = 5
        _DstBlend ("Original destination blend", Float) = 1
        _MASK_Texture ("Original mask", 2D) = "white" {}
        _Mask_Channel_Select ("Original mask channel", Vector) = (1,0,0,0)
        _Main_Texture_Channel_Select ("Original main channel", Vector) = (1,0,0,0)
        _Animation_Alpha ("Original animation alpha", Float) = 1
        _Animator_Alpha ("Original animator alpha", Float) = 1
        _Alpha ("Original alpha", Float) = 1
        _Alphapow ("Original alpha power", Float) = 1
        _Opacity ("Original opacity", Float) = 1
        _Main_Tile_X ("Original U tile", Float) = 1
        _Main_Tile_Y ("Original V tile", Float) = 1
        _Offset_X ("Original U offset", Float) = 0
        _Offset_Y ("Original V offset", Float) = 0
        _Main_Noise_Scroll_X ("Original U scroll", Float) = 0
        _Main_Noise_Scroll_Y ("Original V scroll", Float) = 0
        _RING_ON ("Original ring switch", Float) = 0
        _RING_Radial ("Original ring radius", Float) = 0
        _RING_Blur ("Original ring blur", Float) = 5
        _Radial_Scale ("Original radial scale", Float) = 1
        _BaseBlur ("Original falloff", Float) = 1
        _Noise_Amount ("Original noise scale", Float) = 5
        _Noise_Speed ("Original noise speed", Float) = .1
        _Width ("Original width", Float) = 0
        _Width_Squeeze ("Original squeeze", Float) = 0
        _ALL_MASK_Size ("Original mask size", Float) = 1
        _ALL_MASK_Step ("Original mask soft edge", Float) = .8
        _Color_01 ("Original first color", Color) = (0,1,0,0)
        _Color_02 ("Original second color", Color) = (1,0,0,0)
        _Color_Value ("Original color mix", Float) = 0
        _Color_Noise_Amount ("Original color noise", Float) = 5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off ZWrite Off Blend [_SrcBlend] [_DstBlend]
            HLSLPROGRAM
            #pragma vertex BattleVert
            #pragma fragment frag
            #include "MDPro3BattleSurface.hlsl"
            TEXTURE2D(_SourceTexture); SAMPLER(sampler_SourceTexture);
            TEXTURE2D(_MASK_Texture); SAMPLER(sampler_MASK_Texture);
            CBUFFER_START(UnityPerMaterial)
            float4 _SourceTexture_ST, _MASK_Texture_ST, _SourceTint, _Mask_Channel_Select, _Main_Texture_Channel_Select, _Color_01, _Color_02;
            float _SourceHasTexture, _Family, _SrcBlend, _DstBlend;
            float _Animation_Alpha, _Animator_Alpha, _Alpha, _Alphapow, _Opacity;
            float _Main_Tile_X, _Main_Tile_Y, _Offset_X, _Offset_Y, _Main_Noise_Scroll_X, _Main_Noise_Scroll_Y;
            float _RING_ON, _RING_Radial, _RING_Blur, _Radial_Scale, _BaseBlur, _Noise_Amount, _Noise_Speed, _Width, _Width_Squeeze;
            float _ALL_MASK_Size, _ALL_MASK_Step, _Color_Value, _Color_Noise_Amount;
            CBUFFER_END
            half4 frag(BattleVaryings input) : SV_Target
            {
                float2 uv = input.uv * float2(_Main_Tile_X,_Main_Tile_Y) + float2(_Offset_X,_Offset_Y) + _Time.y * float2(_Main_Noise_Scroll_X,_Main_Noise_Scroll_Y);
                half4 sourceSample = SAMPLE_TEXTURE2D(_SourceTexture,sampler_SourceTexture,uv * _SourceTexture_ST.xy + _SourceTexture_ST.zw);
                half mask = dot(SAMPLE_TEXTURE2D(_MASK_Texture,sampler_MASK_Texture,input.uv * _MASK_Texture_ST.xy + _MASK_Texture_ST.zw),_Mask_Channel_Select);
                float2 centered = (input.uv-.5) * 2;
                float radius = length(centered) * _Radial_Scale;
                half radial = pow(saturate(1-radius),max(_BaseBlur,.1));
                half ring = saturate(1-abs(radius-saturate(1+_RING_Radial)) * max(_RING_Blur,1));
                half alpha = lerp(radial,ring,_RING_ON);
                half3 color = _SourceTint.rgb;
                if (_Family > 1.5 && _Family < 2.5)
                    alpha = pow(saturate(1-abs(centered.x) - _Width),2) * saturate(sin(input.uv.y * PI));
                if (_Family > 2.5)
                {
                    float flow = .5 + .5*sin(uv.x * _Noise_Amount + sin(uv.y * _Color_Noise_Amount + _Time.y * _Noise_Speed));
                    alpha = radial * flow;
                    color = lerp(_Color_01.rgb,_Color_02.rgb,saturate(flow + _Color_Value));
                }
                alpha = lerp(alpha, sourceSample.a * dot(sourceSample,_Main_Texture_Channel_Select),_SourceHasTexture);
                color *= lerp(1,sourceSample.rgb,_SourceHasTexture);
                alpha = pow(saturate(alpha),max(_Alphapow,.1)) * mask * _Animation_Alpha * _Animator_Alpha * _Alpha * _Opacity * input.color.a;
                return half4(color * input.color.rgb,alpha);
            }
            ENDHLSL
        }
    }
}
