Shader "TCG/UI/AvatarComposite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Atlas", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float4 layers:TEXCOORD1; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 layers:TEXCOORD1; float2 position:TEXCOORD2; };
            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            v2f vert(appdata v)
            {
                v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.position=v.vertex.xy;
                o.color=v.color*_Color; o.uv=v.uv; o.layers=v.layers; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 avatar=tex2D(_MainTex,i.uv);
                fixed4 frame=tex2D(_MainTex,i.layers.xy);
                avatar.a*=tex2D(_MainTex,i.layers.zw).a;
                fixed4 result;
                result.rgb=frame.rgb*frame.a+avatar.rgb*avatar.a*(1-frame.a);
                result.a=frame.a+avatar.a*(1-frame.a);
                result.rgb*=i.color.rgb*i.color.a; result.a*=i.color.a;
                #ifdef UNITY_UI_CLIP_RECT
                result*=UnityGet2DClipping(i.position,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a-.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
}
