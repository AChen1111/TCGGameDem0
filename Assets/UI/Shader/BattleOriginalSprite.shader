Shader "TCG/Battle/OriginalSprite"
{
    Properties { _BaseMap("Original sprite",2D)="white"{} }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForwardOnly" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            V Vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.uv=a.uv;o.color=a.color;return o;}
            half4 Frag(V i):SV_Target{return SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*i.color;}
            ENDHLSL
        }
    }
}
