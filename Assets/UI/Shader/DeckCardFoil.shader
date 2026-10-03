Shader "TCG/UI/DeckCardFoil"
{
    Properties
    {
        [PerRendererData] _MainTex ("Card Art", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Rarity ("Version", Float) = 0
        _GoldNoiseMap ("Gold Noise", 2D) = "gray" {}
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
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
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float2 localUv:TEXCOORD1; float4 atlasRect:TEXCOORD2; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float2 localUv:TEXCOORD1; float4 atlasRect:TEXCOORD2; float4 mask:TEXCOORD3; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MainTex, _GoldNoiseMap;
            float4 _Color, _ClipRect, _TextureSampleAdd, _MainTex_TexelSize;
            float _Rarity, _UIMaskSoftnessX, _UIMaskSoftnessY;
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color;
                o.localUv=v.localUv; o.atlasRect=v.atlasRect;
                float2 pixelSize=o.vertex.w / abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 rect=clamp(_ClipRect,-2e10,2e10);
                o.mask=float4(v.vertex.xy*2-rect.xy-rect.zw, .25/(.25*float2(_UIMaskSoftnessX,_UIMaskSoftnessY)+abs(pixelSize)));
                return o;
            }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float3 palette(float t) { return .5+.5*cos(6.2831853*(t+float3(0,.33,.67))); }
            half4 frag(v2f i):SV_Target
            {
                half4 art=tex2D(_MainTex,i.uv)+_TextureSampleAdd;
                float time=_Time.y;
                float sweep=pow(saturate(1-abs(i.localUv.x+i.localUv.y*.35-(frac(time*.19)*1.75-.2))/.18),2.8);
                float e=min(min(i.localUv.x,1-i.localUv.x),min(i.localUv.y,1-i.localUv.y));
                float edge=1-smoothstep(0,.035,e);
                if (_Rarity > .5 && _Rarity < 1.5)
                {
                    float3 rainbow=palette(i.localUv.x*5+i.localUv.y*1.75+time*.2);
                    float stripe=saturate(.35+.65*sin((i.localUv.x*2.2+i.localUv.y*.8+time*.15)*6.2831853));
                    art.rgb=lerp(art.rgb,art.rgb*.65+rainbow*.9,.4*(stripe*.65+sweep*.5));
                    art.rgb+=rainbow*sweep*.16;
                }
                else if (_Rarity > 1.5 && _Rarity < 2.5)
                {
                    float2 grid=floor(i.localUv*8);
                    float2 offset=float2(hash(grid),hash(grid+17.13))*2-1;
                    float2 f=frac(i.localUv*8)-.5;
                    float crack=pow(1-saturate(min(abs(f.x),abs(f.y))*16),10);
                    // 镜碎偏移以单张卡局部UV计算，夹在半像素内边界，再映射回图集。
                    float2 inset=.5*_MainTex_TexelSize.xy/i.atlasRect.zw;
                    float2 distorted=clamp(i.localUv+offset*.014,inset,1-inset);
                    art.rgb=tex2D(_MainTex,i.atlasRect.xy+distorted*i.atlasRect.zw).rgb;
                    art.rgb=lerp(art.rgb,art.rgb*1.2+.1,crack*.5);
                    art.rgb+=sweep*(.07+hash(grid)*.13);
                }
                else if (_Rarity > 2.5)
                {
                    float3 gold=float3(1.45,1.02,.32);
                    art.rgb=lerp(art.rgb,gold,edge*.85);
                    art.rgb+=gold*sweep*(.1+edge*.3);
                    if (_Rarity > 3.5)
                    {
                        float4 n=tex2D(_GoldNoiseMap,i.localUv*3.2);
                        float grain=pow(saturate((frac(n.r*17.27+n.g*9.13+n.b*5.41)-.87)*12),2);
                        float glitter=grain*(.65+.5*sin(time*2.2+hash(floor(i.localUv*150))*20));
                        art.rgb=lerp(art.rgb,gold*1.3,saturate(glitter*.65));
                    }
                }
                art*=i.color;
                #ifdef UNITY_UI_CLIP_RECT
                float2 m=saturate((_ClipRect.zw-_ClipRect.xy-abs(i.mask.xy))*i.mask.zw);
                art.a*=m.x*m.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(art.a-.001);
                #endif
                return art;
            }
            ENDCG
        }
    }
}
