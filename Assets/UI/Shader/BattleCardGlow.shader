Shader "TCG/Battle/CardGlow"
{
    Properties { _EffectAvailable("Available",Float)=0 _Selected("Selected",Float)=0 _Hint("Target",Float)=0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+5" }
        Pass
        {
            Tags { "LightMode"="UniversalForwardOnly" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
            struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            CBUFFER_START(UnityPerMaterial)
            float _EffectAvailable,_Selected,_Hint;
            CBUFFER_END
            V Vert(A a){V o;a.positionOS.xz*=1.12;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.uv=a.uv;return o;}
            half4 Frag(V i):SV_Target
            {
                float d=max(abs(i.uv.x-.5)*2,abs(i.uv.y-.5)*2);
                float rim=smoothstep(.84,.9,d)*(1-smoothstep(.92,1,d));
                float pulse=.7+.3*sin(_Time.y*4);
                float3 gold=float3(1,.78,.12),cyan=float3(.05,.8,1);
                float3 color=gold*(_EffectAvailable*pulse+_Hint)+cyan*_Selected;
                return half4(color*2,rim*saturate(_EffectAvailable+_Selected+_Hint));
            }
            ENDHLSL
        }
    }
}
