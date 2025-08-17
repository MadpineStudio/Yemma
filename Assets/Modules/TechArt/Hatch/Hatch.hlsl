#ifndef HATCHING_INCLUDED
#define HATCHING_INCLUDED


void Posterize(float value, float Steps, out float Out)
{
    Out = floor(value / (1 / Steps)) * (1 / Steps);
}

void hatch_float(float3 Position, float3 Normal, float3 LightDir, float2 UV, float HatchScale, UnityTexture2D hatchTex, out float Out)
{
    float lightGradientWS = dot(-LightDir, Normal);
    Posterize(lightGradientWS, 2, lightGradientWS);
    float hatchGradient = tex2D(hatchTex, UV * HatchScale).r;
    lightGradientWS = lerp(hatchGradient, 1, lightGradientWS);
    Out = lightGradientWS;
}
#endif

