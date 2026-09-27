Shader "TCG/UI/DeckBackground"
{
    Properties { _MainTex ("Texture",2D)="white" {} _Panel ("Panel",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _Panel;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            v2f vert(appdata i) { v2f o; o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o; }
            half4 frag(v2f i):SV_Target
            {
                float3 color=lerp(float3(.06,.006,.095),float3(.003,.006,.013),i.uv.y);
                float diagonal=frac(i.uv.x*.95-i.uv.y*.6+.1);
                color+=step(.4,diagonal)*step(diagonal,.82)*float3(.018,.027,.07);
                color=lerp(color,lerp(float3(.006,.024,.042),float3(.002,.008,.013),abs(i.uv.x-.4)*1.5),_Panel);
                color*=.9+.1*sin(i.uv.y*960*3.14159);
                return half4(color,1)*i.color;
            }
            ENDCG
        }
    }
}
