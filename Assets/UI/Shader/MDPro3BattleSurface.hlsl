#ifndef MDPRO3_BATTLE_SURFACE_INCLUDED
#define MDPRO3_BATTLE_SURFACE_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
struct BattleAttributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
struct BattleVaryings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; float3 positionWS : TEXCOORD1; float3 positionOS : TEXCOORD2; };
BattleVaryings BattleVert(BattleAttributes input)
{
    BattleVaryings output;
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.uv = input.uv;
    output.color = input.color;
    output.positionOS = input.positionOS.xyz;
    return output;
}
half BattleShadow(float3 worldPosition)
{
    Light light = GetMainLight(TransformWorldToShadowCoord(worldPosition));
    return lerp(0.65, 1.0, light.shadowAttenuation);
}
#endif
